from zk import ZK, const
import sys

def connect_to_zkteco(ip_address, port=4370):
    """
    Menghubungkan ke perangkat ZKTeco dan mengembalikan objek koneksi.
    """
    zk = ZK(ip_address, port=port, timeout=5, password=0, force_udp=False, ommit_ping=False)
    try:
        print(f"Menghubungkan ke perangkat ZKTeco di {ip_address}:{port}...")
        conn = zk.connect()
        print("Berhasil terhubung!")
        return conn
    except Exception as e:
        print(f"Gagal terhubung ke perangkat ZKTeco: {e}")
        sys.exit(1)

def play_voice(conn):
    """
    Memutar suara bawaan perangkat ZKTeco.
    """
    try:
        print("Memutar suara bawaan perangkat...")
        conn.test_voice()
        print("Suara berhasil diputar.")
    except Exception as e:
        print(f"Gagal memutar suara: {e}")

def disconnect_from_zkteco(conn):
    """
    Memutuskan koneksi dari perangkat ZKTeco.
    """
    try:
        conn.disconnect()
        print("Koneksi berhasil diputus.")
    except Exception as e:
        print(f"Gagal memutuskan koneksi: {e}")

if __name__ == "__main__":
    IP_ADDRESS = "192.168.1.201"  # IP statis perangkat ZKTeco
    PORT = 4370  # Port default ZKTeco

    # Hubungkan ke perangkat
    connection = connect_to_zkteco(IP_ADDRESS, PORT)

    # Memutar suara bawaan perangkat jika koneksi berhasil
    play_voice(connection)

    # Memutuskan koneksi
    disconnect_from_zkteco(connection)
