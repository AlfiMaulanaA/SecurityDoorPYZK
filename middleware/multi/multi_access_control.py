import json
import paho.mqtt.client as mqtt
from zk import ZK, const
from threading import Thread

# Load configuration
config_file = 'config_access_control.json'

def load_config():
    with open(config_file, 'r') as f:
        return json.load(f)

def save_config(config):
    with open(config_file, 'w') as f:
        json.dump(config, f, indent=4)

config = load_config()
devices = config["devices"]
mqtt_broker = config["mqtt_broker"]
mqtt_port = config["mqtt_port"]
mqtt_topic_command = config["mqtt_topic_command"]
mqtt_topic_response = config["mqtt_topic_response"]
mqtt_topic_attendance_response = config["mqtt_topic_attendance_response"]

# Identify master device
master_device = next((device for device in devices if device.get("is_master")), None)
if not master_device:
    raise Exception("No master device found in the configuration.")

# Initialize master ZKTeco connection
zk_master = ZK(master_device["ip"], port=master_device["port"], timeout=5, password=0, force_udp=False, ommit_ping=False)

def on_connect(client, userdata, flags, rc):
    print("Connected to MQTT broker")
    client.subscribe(mqtt_topic_command)

def on_message(client, userdata, msg):
    global zk_master
    data = json.loads(msg.payload)
    command = data.get("command", "")
    Thread(target=execute_command_on_master, args=(zk_master, command, data)).start()

def execute_command_on_master(zk, command, data):
    conn = None
    response = {"response": "failure", "command": command}
    try:
        conn = zk.connect()
        conn.disable_device()

        if command == "add_user":
            username = data["username"]
            password = data["password"]
            users = conn.get_users()
            max_uid = max([user.uid for user in users], default=0)
            new_uid = max_uid + 1
            conn.set_user(uid=new_uid, name=username, privilege=const.USER_DEFAULT, password=password, group_id='', user_id=str(new_uid))
            user_added = any(user.uid == new_uid for user in conn.get_users())
            if user_added:
                conn.test_voice(index=0)
                response = {"response": "success", "command": "add_user", "uid": new_uid, "username": username, "password": password}
                sync_to_slaves("add_user", response)
            else:
                conn.test_voice(index=4)
                response = {"response": "failure", "command": "add_user", "uid": new_uid, "username": username, "password": password, "error": "User not added"}
        elif command == "edit_user":
            uid = data["uid"]
            username = data["username"]
            password = data["password"]
            users = conn.get_users()
            user = next((user for user in users if user.uid == uid), None)
            if user:
                conn.set_user(uid=uid, name=username, privilege=user.privilege, password=password, group_id=user.group_id, user_id=user.user_id)
                user_edited = any(user.uid == uid and user.name == username and user.password == password for user in conn.get_users())
                if user_edited:
                    conn.test_voice(index=0)
                    response = {"response": "success", "command": "edit_user", "uid": uid, "username": username, "password": password}
                    sync_to_slaves("edit_user", response)
                else:
                    conn.test_voice(index=4)
                    response = {"response": "failure", "command": "edit_user", "uid": uid, "username": username, "password": password, "error": "User not edited"}
            else:
                conn.test_voice(index=3)
                response = {"response": "failure", "command": "edit_user", "uid": uid, "username": username, "password": password, "error": "User not found"}
        elif command == "delete_user":
            uid = data["uid"]
            conn.delete_user(uid=uid)
            user_deleted = not any(user.uid == uid for user in conn.get_users())
            if user_deleted:
                conn.test_voice(index=0)
                response = {"response": "success", "command": "delete_user", "uid": uid}
                sync_to_slaves("delete_user", response)
            else:
                conn.test_voice(index=4)
                response = {"response": "failure", "command": "delete_user", "uid": uid, "error": "User not deleted"}
        elif command == "clear_users":
            conn.clear_data()
            conn.test_voice(index=0)
            response = {"response": "success", "command": "clear_users"}
            sync_to_slaves("clear_users", response)
        elif command == "enroll_finger":
            uid = data["uid"]
            finger_index = data["finger_index"]
            try:
                conn.enroll_user(uid=uid, temp_id=finger_index)
                response = {"response": "success", "command": "enroll_finger", "uid": uid, "finger_index": finger_index}
                conn.test_voice(index=0)
                sync_to_slaves("enroll_finger", response)
            except Exception as e:
                response = {"response": "failure", "command": "enroll_finger", "uid": uid, "finger_index": finger_index, "error": str(e)}
                conn.test_voice(index=4)
        elif command == "delete_finger":
            uid = data["uid"]
            finger_index = data["finger_index"]
            try:
                conn.delete_user_template(uid=uid, temp_id=finger_index)
                response = {"response": "success", "command": "delete_finger", "uid": uid, "finger_index": finger_index}
                conn.test_voice(index=0)
                sync_to_slaves("delete_finger", response)
            except Exception as e:
                response = {"response": "failure", "command": "delete_finger", "uid": uid, "finger_index": finger_index, "error": str(e)}
                conn.test_voice(index=4)
        elif command == "register_card":
            uid = data["uid"]
            card = data["card"]
            try:
                conn.set_user(uid=uid, card=card)
                response = {"response": "success", "command": "register_card", "uid": uid, "card": card}
                conn.test_voice(index=0)
                sync_to_slaves("register_card", response)
            except Exception as e:
                response = {"response": "failure", "command": "register_card", "uid": uid, "card": card, "error": str(e)}
                conn.test_voice(index=4)
        elif command == "delete_card":
            uid = data["uid"]
            try:
                conn.set_user(uid=uid, card=0)
                response = {"response": "success", "command": "delete_card", "uid": uid}
                conn.test_voice(index=0)
                sync_to_slaves("delete_card", response)
            except Exception as e:
                response = {"response": "failure", "command": "delete_card", "uid": uid, "error": str(e)}
                conn.test_voice(index=4)
        elif command == "get_attendance":
            try:
                attendances = conn.get_attendance()
                if attendances:
                    attendance_data = [{"uid": att.uid, "timestamp": str(att.timestamp)} for att in attendances]
                    attendance_response = {"response": "success", "command": "get_attendance", "attendance": attendance_data}
                    conn.test_voice(index=0)
                    print("Attendance data retrieved successfully")
                else:
                    attendance_response = {"response": "success", "command": "get_attendance", "attendance": "{no_attendance_tracking}"}
                    print("No attendance data available")
            except Exception as e:
                attendance_response = {"response": "failure", "command": "get_attendance", "error": str(e)}
                conn.test_voice(index=4)
                print(f"Failed to retrieve attendance data - Error: {str(e)}")
        elif command == "clear_attendance":
            try:
                conn.clear_attendance()
                attendance_response = {"response": "success", "command": "clear_attendance"}
                conn.test_voice(index=0)
                print("All attendance records cleared successfully")
            except Exception as e:
                attendance_response = {"response": "failure", "command": "clear_attendance", "error": str(e)}
                conn.test_voice(index=4)
                print(f"Failed to clear attendance records - Error: {str(e)}")
        elif command == "get_all_users":
            try:
                users = conn.get_users()
                if users:
                    user_data = [{"uid": user.uid, "username": user.name, "password": user.password, "privilege": user.privilege, "user_id": user.user_id, "card": user.card} for user in users]
                    response = {"response": "success", "command": "get_all_users", "users": user_data}
                    conn.test_voice(index=0)
                    print("User data retrieved successfully")
                else:
                    response = {"response": "success", "command": "get_all_users", "users": []}
                    print("No users available")
            except Exception as e:
                response = {"response": "failure", "command": "get_all_users", "error": str(e)}
                conn.test_voice(index=4)
                print(f"Failed to retrieve user data - Error: {str(e)}")
    except Exception as e:
        print(f"Error: {e}")
        response = {"response": "failure", "command": command, "error": str(e)}
        conn.test_voice(index=4)
    finally:
        if conn:
            conn.enable_device()
            conn.disconnect()
        client.publish(mqtt_topic_response, json.dumps(response))
        if command in ["get_attendance", "clear_attendance"]:
            client.publish(mqtt_topic_attendance_response, json.dumps(attendance_response))

def sync_to_slaves(command, data):
    for device in devices:
        if not device.get("is_master"):
            zk = ZK(device["ip"], port=device["port"], timeout=5, password=0, force_udp=False, ommit_ping=False)
            Thread(target=execute_command_on_slave, args=(zk, command, data)).start()

def execute_command_on_slave(zk, command, data):
    conn = None
    try:
        conn = zk.connect()
        conn.disable_device()
        
        if command == "add_user":
            uid = data["uid"]
            username = data["username"]
            password = data["password"]
            conn.set_user(uid=uid, name=username, privilege=const.USER_DEFAULT, password=password, group_id='', user_id=str(uid))
            print(f"Synchronized add_user to {zk.ip}")
        elif command == "edit_user":
            uid = data["uid"]
            username = data["username"]
            password = data["password"]
            users = conn.get_users()
            user = next((user for user in users if user.uid == uid), None)
            if user:
                conn.set_user(uid=uid, name=username, privilege=user.privilege, password=password, group_id=user.group_id, user_id=user.user_id)
                print(f"Synchronized edit_user to {zk.ip}")
        elif command == "delete_user":
            uid = data["uid"]
            conn.delete_user(uid=uid)
            print(f"Synchronized delete_user to {zk.ip}")
        elif command == "clear_users":
            conn.clear_data()
            print(f"Synchronized clear_users to {zk.ip}")
        elif command == "enroll_finger":
            uid = data["uid"]
            finger_index = data["finger_index"]
            conn.enroll_user(uid=uid, temp_id=finger_index)
            print(f"Synchronized enroll_finger to {zk.ip}")
        elif command == "delete_finger":
            uid = data["uid"]
            finger_index = data["finger_index"]
            conn.delete_user_template(uid=uid, temp_id=finger_index)
            print(f"Synchronized delete_finger to {zk.ip}")
        elif command == "register_card":
            uid = data["uid"]
            card = data["card"]
            conn.set_user(uid=uid, card=card)
            print(f"Synchronized register_card to {zk.ip}")
        elif command == "delete_card":
            uid = data["uid"]
            conn.set_user(uid=uid, card=0)
            print(f"Synchronized delete_card to {zk.ip}")
        # Tambahkan perintah sinkronisasi lainnya di sini
    except Exception as e:
        print(f"Error syncing to slave {zk.ip}: {e}")
    finally:
        if conn:
            conn.enable_device()
            conn.disconnect()

client = mqtt.Client()
client.on_connect = on_connect
client.on_message = on_message

client.connect(mqtt_broker, mqtt_port, 60)

# Function to handle live attendance capture
def handle_live_capture():
    for device in devices:
        if device.get("is_master"):
            capture_thread = Thread(target=capture_attendance, args=(device,))
            capture_thread.start()
        else:
            capture_thread = Thread(target=capture_attendance, args=(device,))
            capture_thread.start()

def capture_attendance(device):
    zk = ZK(device["ip"], port=device["port"], timeout=5, password=0, force_udp=False, ommit_ping=False)
    conn = None
    try:
        conn = zk.connect()
        print(f"Starting live capture for attendance on {device['id']}...")
        for attendance in conn.live_capture():
            if attendance is None:
                continue
            attendance_data = {
                "uid": attendance.uid,
                "timestamp": str(attendance.timestamp),
                "device_id": device["id"]
            }
            client.publish(mqtt_topic_attendance_response, json.dumps({
                "response": "success",
                "command": "live_attendance",
                "attendance": attendance_data
            }))
            print(f"Live attendance captured on {device['id']}: {attendance_data}")
    except Exception as e:
        print(f"Error during live capture on {device['id']}: {e}")
    finally:
        if conn:
            conn.disconnect()

handle_live_capture()

client.loop_forever()
