import json
import paho.mqtt.client as mqtt
from zk import ZK, const
import threading
from threading import Lock

# Load configuration
config_file = 'config_access_control.json'

def load_config():
    with open(config_file, 'r') as f:
        return json.load(f)

def save_config(config):
    with open(config_file, 'w') as f:
        json.dump(config, f, indent=4)

config = load_config()

# ZKTeco connection settings
zkteco_ip = config["zkteco_ip"]
zkteco_port = config["zkteco_port"]
mqtt_broker = config["mqtt_broker"]
mqtt_port = config["mqtt_port"]
mqtt_topic_command = config["mqtt_topic_command"]
mqtt_topic_response = config["mqtt_topic_response"]
mqtt_topic_attendance_response = config["mqtt_topic_attendance_response"]

zk = ZK(zkteco_ip, port=zkteco_port, timeout=5, password=0, force_udp=False, ommit_ping=False)

def reconnect_device():
    """
    Mencoba menyambungkan kembali ke perangkat ZKTeco jika koneksi gagal.
    """
    global zk
    try:
        print("Reconnecting to device...")
        conn = zk.connect()
        print("Reconnected to device.")
        return conn
    except Exception as e:
        print(f"Failed to reconnect to device: {e}")
        return None

def on_connect(client, userdata, flags, rc):
    print("Connected to MQTT broker")
    client.subscribe(mqtt_topic_command)

def get_user_name_by_uid(uid):
    try:
        users = conn.get_users()  # Ambil daftar semua pengguna
        user = next((user for user in users if user.uid == uid), None)
        return user.name if user else "Unknown"
    except Exception as e:
        print(f"Error fetching user name for UID {uid}: {e}")
        return "Unknown"

def on_message(client, userdata, msg):
    global zk, zkteco_ip
    conn = None
    response = {"response": "failure", "command": "", "uid": None, "username": None, "password": None, "finger_index": None, "card": None, "attendance": None}
    attendance_response = {"response": "failure", "command": "", "attendance": None}
    try:
        data = json.loads(msg.payload)
        command = data.get("command", "")
        
        if command == "update_zkteco_ip":
            new_ip = data["zkteco_ip"]
            config["zkteco_ip"] = new_ip
            save_config(config)
            zk = ZK(new_ip, port=zkteco_port, timeout=5, password=0, force_udp=False, ommit_ping=False)
            response = {"response": "success", "command": "update_zkteco_ip", "zkteco_ip": new_ip}
            print(f"Updated ZKTeco IP to: {new_ip}")
            client.publish(mqtt_topic_response, json.dumps(response))
            return
        
        conn = zk.connect()
        conn.disable_device()
        
        if command == "add_user":
            username = data["username"]
            password = data["password"]

            # Get the highest existing uid and increment it by one
            users = conn.get_users()
            max_uid = max([user.uid for user in users], default=0)
            new_uid = max_uid + 1

            conn.set_user(uid=new_uid, name=username, privilege=const.USER_DEFAULT, password=password, group_id='', user_id=str(new_uid))

            # Validate if the user was added
            user_added = any(user.uid == new_uid for user in conn.get_users())
            if user_added:
                conn.test_voice(index=0)  # Play "Thank You" voice message
                response = {"response": "success", "command": "add_user", "uid": new_uid, "username": username, "password": password}
                print(f"Added user: {username}")
            else:
                response = {"response": "failure", "command": "add_user", "uid": new_uid, "username": username, "password": password, "error": "User not added"}
                conn.test_voice(index=4)  # Play "Please try again" voice message
        elif command == "clear_users":
            conn.clear_data()
            conn.test_voice(index=0)  # Play "Thank You" voice message
            response = {"response": "success", "command": "clear_users"}
            print("Cleared all users")
        elif command == "delete_user":
            uid = data["uid"]
            conn.delete_user(uid=uid)
            # Validate if the user was deleted
            user_deleted = not any(user.uid == uid for user in conn.get_users())
            if user_deleted:
                conn.test_voice(index=0)  # Play "Thank You" voice message
                response = {"response": "success", "command": "delete_user", "uid": uid}
                print(f"Deleted user with UID: {uid}")
            else:
                response = {"response": "failure", "command": "delete_user", "uid": uid, "error": "User not deleted"}
                conn.test_voice(index=4)  # Play "Please try again" voice message
        elif command == "edit_user":
            uid = data["uid"]
            username = data["username"]
            password = data["password"]
            users = conn.get_users()
            user = next((user for user in users if user.uid == uid), None)
            if user:
                conn.set_user(uid=uid, name=username, privilege=user.privilege, password=password, group_id=user.group_id, user_id=user.user_id)
                # Validate if the user was edited
                user_edited = any(user.uid == uid and user.name == username and user.password == password for user in conn.get_users())
                if user_edited:
                    conn.test_voice(index=0)  # Play "Thank You" voice message
                    response = {"response": "success", "command": "edit_user", "uid": uid, "username": username, "password": password}
                    print(f"Edited user with UID: {uid}")
                else:
                    response = {"response": "failure", "command": "edit_user", "uid": uid, "username": username, "password": password, "error": "User not edited"}
                    conn.test_voice(index=4)  # Play "Please try again" voice message
            else:
                response = {"response": "failure", "command": "edit_user", "uid": uid, "username": username, "password": password, "error": "User not found"}
                conn.test_voice(index=3)  # Play "Invalid ID" voice message
        if command == "enroll_finger":
            uid = data["uid"]
            finger_index = data["finger_index"]
            mode = data.get("mode", "register")  # Default to 'register' mode

            if mode == "register":
                # Mode Register: Send message indicating the device is ready to receive fingerprint
                response = {"response": "success", "command": "enroll_finger", "uid": uid, "finger_index": finger_index, "mode": "scan"}
                print(f"Ready to enroll fingerprint for UID: {uid} and Finger Index: {finger_index}")
                client.publish(mqtt_topic_response, json.dumps(response))
            
            elif mode == "scan":
                # Mode Scan: Wait for user to place their finger on the sensor
                print("Please Tap The Finger on the Green Box")
                response = {"response": "success", "command": "enroll_finger", "uid": uid, "finger_index": finger_index, "mode": "save", "message": "Please Tap The Finger on the Green Box"}
                client.publish(mqtt_topic_response, json.dumps(response))
                
                # Simulate the fingerprint scanning process
                try:
                    conn.enroll_user(uid=uid, temp_id=finger_index)  # Enroll the fingerprint
                    print(f"Fingerprint enrolled for UID: {uid} at index {finger_index}")
                    
                    # Mode Save: If fingerprint is enrolled successfully, save it
                    response = {"response": "success", "command": "enroll_finger", "uid": uid, "finger_index": finger_index, "mode": "save", "message": "Fingerprint saved successfully"}
                    client.publish(mqtt_topic_response, json.dumps(response))
                except Exception as e:
                    response = {"response": "failure", "command": "enroll_finger", "uid": uid, "finger_index": finger_index, "error": str(e)}
                    print(f"Failed to enroll fingerprint for UID: {uid}, Error: {str(e)}")
                    client.publish(mqtt_topic_response, json.dumps(response))
            else:
                response = {"response": "failure", "command": "enroll_finger", "error": "Invalid mode"}
                client.publish(mqtt_topic_response, json.dumps(response))
        elif command == "delete_finger":
            uid = data["uid"]
            finger_index = data["finger_index"]
            try:
                conn.delete_user_template(uid=uid, temp_id=finger_index)
                response = {"response": "success", "command": "delete_finger", "uid": uid, "finger_index": finger_index}
                conn.test_voice(index=0)  # Play "Thank You" voice message
                print(f"Deleted finger {finger_index} for user with UID: {uid}")
            except Exception as e:
                response = {"response": "failure", "command": "delete_finger", "uid": uid, "finger_index": finger_index, "error": str(e)}
                conn.test_voice(index=4)  # Play "Please try again" voice message
                print(f"Failed to delete finger {finger_index} for user with UID: {uid} - Error: {str(e)}")
        elif command == "register_card":
            uid = data["uid"]
            card = data["card"]
            try:
                conn.set_user(uid=uid, card=card)
                response = {"response": "success", "command": "register_card", "uid": uid, "card": card}
                conn.test_voice(index=0)  # Play "Thank You" voice message
                print(f"Registered card {card} for user with UID: {uid}")
            except Exception as e:
                response = {"response": "failure", "command": "register_card", "uid": uid, "card": card, "error": str(e)}
                conn.test_voice(index=4)  # Play "Please try again" voice message
                print(f"Failed to register card {card} for user with UID: {uid} - Error: {str(e)}")
        elif command == "delete_card":
            uid = data["uid"]
            try:
                conn.set_user(uid=uid, card=0)
                response = {"response": "success", "command": "delete_card", "uid": uid}
                conn.test_voice(index=0)  # Play "Thank You" voice message
                print(f"Deleted card for user with UID: {uid}")
            except Exception as e:
                response = {"response": "failure", "command": "delete_card", "uid": uid, "error": str(e)}
                conn.test_voice(index=4)  # Play "Please try again" voice message
                print(f"Failed to delete card for user with UID: {uid} - Error: {str(e)}")
        elif command == "get_attendance":
            try:
                # Ambil data kehadiran langsung dari perangkat ZKTeco
                attendances = conn.get_attendance()  # Fungsi ini mengembalikan data kehadiran dari perangkat ZKTeco
                if attendances:
                    # Format data kehadiran menjadi JSON asli
                    attendance_data = [
                        {
                            "uid": att.uid,  # UID pengguna yang melakukan absensi
                            "username": get_user_name_by_uid(att.uid),  # Mendapatkan nama pengguna berdasarkan UID
                            "timestamp": str(att.timestamp),  # Waktu absensi (timestamp)
                            "status": att.status if hasattr(att, 'status') else "Unknown",  # Status absensi
                            "punch": att.punch if hasattr(att, 'punch') else "Unknown"  # Jenis absensi
                        }
                        for att in attendances
                    ]

                    # Kirimkan data asli ke MQTT
                    attendance_response = {
                        "response": "success",
                        "command": "get_attendance",
                        "attendance": attendance_data
                    }

                    client.publish(mqtt_topic_attendance_response, json.dumps(attendance_response))
                    conn.test_voice(index=0)  # Play "Thank You" voice message
                    print("Attendance data retrieved and sent to MQTT successfully.")
                else:
                    attendance_response = {
                        "response": "success",
                        "command": "get_attendance",
                        "attendance": []
                    }
                    print("No attendance data available.")
                    client.publish(mqtt_topic_attendance_response, json.dumps(attendance_response))
            except Exception as e:
                attendance_response = {
                    "response": "failure",
                    "command": "get_attendance",
                    "error": str(e)
                }
                conn.test_voice(index=4)  # Play "Please try again" voice message
                print(f"Failed to retrieve attendance data - Error: {str(e)}")
                client.publish(mqtt_topic_attendance_response, json.dumps(attendance_response))

        elif command == "clear_attendance":
            try:
                conn.clear_attendance()
                attendance_response = {"response": "success", "command": "clear_attendance"}
                conn.test_voice(index=0)  # Play "Thank You" voice message
                print("All attendance records cleared successfully")
            except Exception as e:
                attendance_response = {"response": "failure", "command": "clear_attendance", "error": str(e)}
                conn.test_voice(index=4)  # Play "Please try again" voice message
                print(f"Failed to clear attendance records - Error: {str(e)}")
        elif command == "get_all_users":
            try:
                users = conn.get_users()
                if users:
                    user_data = [{"uid": user.uid, "username": user.name, "password": user.password,"user_id": user.user_id, "privilege": user.privilege, "card": user.card} for user in users]
                    response = {"response": "success", "command": "get_all_users", "users": user_data}
                    print("User data retrieved successfully")
                else:
                    response = {"response": "success", "command": "get_all_users", "users": []}
                    print("No users available")
            except Exception as e:
                response = {"response": "failure", "command": "get_all_users", "error": str(e)}
                conn.test_voice(index=4)  # Play "Please try again" voice message
                print(f"Failed to retrieve user data - Error: {str(e)}")

    except TimeoutError:
        print("Timeout error occurred. Reconnecting...")
        response.update({"error": "Timeout error. Device not responding."})
        conn = reconnect_device()

    except Exception as e:
        print(f"Error: {e}")
        response = {"response": "failure", "command": command, "error": str(e)}
        conn.test_voice(index=4)  # Play "Please try again" voice message
    finally:
        if conn:
            conn.enable_device()
            conn.disconnect()
        client.publish(mqtt_topic_response, json.dumps(response))
        if command in ["get_attendance", "clear_attendance"]:
            client.publish(mqtt_topic_attendance_response, json.dumps(attendance_response))

client = mqtt.Client()
client.on_connect = on_connect
client.on_message = on_message

client.connect(mqtt_broker, mqtt_port, 60)

connection_lock = Lock()

def handle_live_capture():
    """
    Menangani live capture data kehadiran.
    """
    conn = None
    users = {}
    try:
        with connection_lock:  # Sinkronisasi koneksi
            conn = zk.connect()
            if not conn:
                raise ConnectionError("Failed to connect to the ZKTeco device.")
            print("Fetching user data...")
            all_users = conn.get_users()
            users = {user.uid: user.name for user in all_users}
            print("User data loaded successfully.")

        print("Starting live capture for attendance...")
        for attendance in conn.live_capture():
            if attendance is None:
                continue

            username = users.get(attendance.uid, "Unknown User")  # Ambil nama pengguna berdasarkan UID
            attendance_data = {
                "uid": attendance.uid,
                "username": username,
                "timestamp": str(attendance.timestamp)
            }
            client.publish(mqtt_topic_attendance_response, json.dumps({
                "response": "success",
                "command": "live_attendance",
                "attendance": attendance_data
            }))
            print(f"Live attendance captured: {attendance_data}")
    except TimeoutError:
        print("Timeout during live capture. Attempting to reconnect...")
        handle_reconnect(users)
    except Exception as e:
        print(f"Error during live capture: {e}")
        handle_reconnect(users)
    finally:
        with connection_lock:  # Pastikan koneksi ditutup dengan aman
            if conn and conn.is_connect:
                conn.disconnect()

def handle_reconnect(users):
    """
    Tangani reconnect pada perangkat ZKTeco.
    """
    global zk
    try:
        with connection_lock:
            conn = reconnect_device()
            if conn:
                all_users = conn.get_users()
                users.clear()
                users.update({user.uid: user.name for user in all_users})
                print("Reconnected and refreshed user data.")
    except Exception as e:
        print(f"Failed to reconnect during live capture: {e}")

# Jalankan live capture dalam thread terpisah
live_capture_thread = threading.Thread(target=handle_live_capture, daemon=True)
live_capture_thread.start()

client.loop_forever()