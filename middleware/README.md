# Access Control System with ZKTeco and MQTT

This project provides an access control system using a ZKTeco device and MQTT communication. The system allows you to manage users, fingerprints, and cards, as well as retrieve attendance data, all via MQTT commands.

## Installation

1.  **Clone the repository:**

    ```sh
    git clone https://github.com/yourusername/access_control_project.git
    cd access_control_project
    ```

2.  **Install required libraries:**

    ```sh
    pip install paho-mqtt pyzk
    ```

3.  **Set up the project directory structure:**

    ```sh
    mkdir service
    ```

4.  **Create the `config_access_control.json` file:**

    ```sh
    nano config_access_control.json
    ```

    Add the following content:

    ```json
    {
      "zkteco_ip": "192.168.1.201",
      "zkteco_port": 4370,
      "mqtt_broker": "localhost",
      "mqtt_port": 1883,
      "mqtt_topic_command": "access_control/command",
      "mqtt_topic_response": "access_control/response",
      "mqtt_topic_attendance_response": "access_control/attendance_response"
    }
    ```

5.  **Create the `access_control.py` file:**

    ```sh
    nano access_control.py
    ```

    Add the following content:

    ```python
    import json
    import paho.mqtt.client as mqtt
    from zk import ZK, const

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

    def on_connect(client, userdata, flags, rc):
        print("Connected to MQTT broker")
        client.subscribe(mqtt_topic_command)

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
            elif command == "enroll_finger":
                uid = data["uid"]
                finger_index = data["finger_index"]
                try:
                    conn.enroll_user(uid=uid, temp_id=finger_index)
                    response = {"response": "success", "command": "enroll_finger", "uid": uid, "finger_index": finger_index}
                    conn.test_voice(index=0)  # Play "Thank You" voice message
                    print(f"Enrolled finger {finger_index} for user with UID: {uid}")
                except Exception as e:
                    response = {"response": "failure", "command": "enroll_finger", "uid": uid, "finger_index": finger_index, "error": str(e)}
                    conn.test_voice(index=4)  # Play "Please try again" voice message
                    print(f"Failed to enroll finger {finger_index} for user with UID: {uid} - Error: {str(e)}")
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
                    attendances = conn.get_attendance()
                    if attendances:
                        attendance_data = [{"uid": att.uid, "timestamp": str(att.timestamp)} for att in attendances]
                        attendance_response = {"response": "success", "command": "get_attendance", "attendance": attendance_data}
                        conn.test_voice(index=0)  # Play "Thank You" voice message
                        print("Attendance data retrieved successfully")
                    else:
                        attendance_response = {"response": "success", "command": "get_attendance", "attendance": "{no_attendance_tracking}"}
                        print("No attendance data available")
                except Exception as e:
                    attendance_response = {"response": "failure", "command": "get_attendance", "error": str(e)}
                    conn.test_voice(index=4)  # Play "Please try again" voice message
                    print(f"Failed to retrieve attendance data - Error: {str(e)}")
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

    # Function to handle live attendance capture
    def handle_live_capture():
        conn = None
        try:
            conn = zk.connect()
            print("Starting live capture for attendance...")
            for attendance in conn.live_capture():
                if attendance is None:
                    continue
                attendance_data = {
                    "uid": attendance.uid,
                    "timestamp": str(attendance.timestamp)
                }
                client.publish(mqtt_topic_attendance_response, json.dumps({
                    "response": "success",
                    "command": "live_attendance",
                    "attendance": attendance_data
                }))
                print(f"Live attendance captured: {attendance_data}")
        except Exception as e:
            print(f"Error during live capture: {e}")
        finally:
            if conn:
                conn.disconnect()

    import threading
    live_capture_thread = threading.Thread(target=handle_live_capture)
    live_capture_thread.start()

    client.loop_forever()
    ```

6.  **Create the Systemd Service File:**

    ```sh
    cd service
    nano access_control.service
    ```

    Add the following content:

    ```ini
    [Unit]
    Description=Access Control Service
    After=multi-user.target
    [Service]
    Type=idle
    ExecStart=/usr/bin/python3 /home/containment/access_control_project/access_control.py
    WorkingDirectory=/home/containment/access_control_project
    [Install]
    WantedBy=multi-user.target
    ```

        ```sh
        sudo systemctl daemon-reload

    sudo systemctl enable access_control.service
    sudo systemctl start access_control.service

````

7. **Enable and Start the Service:**

```sh
sudo cp /home/pi/access_control_project/service/access_control.service /etc/systemd/system/
sudo systemctl daemon-reload
sudo systemctl enable access_control.service
sudo systemctl start access_control.service
````

## Using MQTT to Execute Commands

1. **Add a User**

   ```sh
   mosquitto_pub -h localhost -t "access_control/command" -m '{"command": "add_user", "username": "testuser", "password": "testpass"}'
   ```

   {
   "command": "get_all_users"
   }

2. **Edit a User by UID**

   ```sh
   mosquitto_pub -h localhost -t "access_control/command" -m '{"command": "edit_user", "uid": 1, "username": "newusername", "password": "newpassword"}'
   ```

3. **Delete a User by UID**

   ```sh
   mosquitto_pub -h localhost -t "access_control/command" -m '{"command": "delete_user", "uid": 1}'
   ```

4. **Clear Users**

   ```sh
   mosquitto_pub -h localhost -t "access_control/command" -m '{"command": "clear_users"}'
   ```

5. **Enroll a Fingerprint by UID and Finger Index**

   ```sh
   mosquitto_pub -h localhost -t "access_control/command" -m '{"command": "enroll_finger", "uid": 1, "finger_index": 0}'
   ```

6. **Delete a Fingerprint by UID and Finger Index**

   ```sh
   mosquitto_pub -h localhost -t "access_control/command" -m '{"command": "delete_finger", "uid": 1, "finger_index": 0}'
   ```

7. **Register a Card by UID**

   ```sh
   mosquitto_pub -h localhost -t "access_control/command" -m '{"command": "register_card", "uid": 1, "card": 123456}'
   ```

8. **Delete a Card by UID**

   ```sh
   mosquitto_pub -h localhost -t "access_control/command" -m '{"command": "delete_card", "uid": 1}'
   ```

9. **Get Attendance**

   ```sh
   mosquitto_pub -h localhost -t "access_control/command" -m '{"command": "get_attendance"}'
   ```

10. **Clear Attendance**

    ```sh
    mosquitto_pub -h localhost -t "access_control/command" -m '{"command": "clear_attendance"}'
    ```

11. **Update ZKTeco IP Address**

    ```sh
    mosquitto_pub -h localhost -t "access_control/command" -m '{"command": "update_zkteco_ip", "zkteco_ip": "192.168.1.202"}'
    ```

12. **Subscribe to Responses**

    To receive responses from the MQTT topics, you can subscribe to the response topics:

    ```sh
    mosquitto_sub -h localhost -t "access_control/response"
    ```

    For attendance responses:

    ```sh
    mosquitto_sub -h localhost -t "access_control/attendance_response"
    ```

## Example MQTT Responses

- **Add User Success:**

  ```json
  {
    "response": "success",
    "command": "add_user",
    "uid": 1,
    "username": "testuser",
    "password": "testpass"
  }
  ```

- **Delete User Failure:**

  ```json
  {
    "response": "failure",
    "command": "delete_user",
    "uid": 1,
    "error": "User not deleted"
  }
  ```

- **Live Attendance:**

  ```json
  {
    "response": "success",
    "command": "live_attendance",
    "attendance": {
      "uid": 1,
      "timestamp": "2024-07-31 10:29:58"
    }
  }
  ```

- **Update ZKTeco IP Success:**

  ```json
  {
    "response": "success",
    "command": "update_zkteco_ip",
    "zkteco_ip": "192.168.1.202"
  }
  ```

This setup will allow your Raspberry Pi to control the ZKTeco device via MQTT using a single command topic. The `command` field in the payload will specify the action to be performed, and the responses will be published to the appropriate response topics. Additionally, it includes the feature to update the ZKTeco device IP address using an MQTT command.
