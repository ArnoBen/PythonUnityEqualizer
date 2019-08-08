import socket
import json
class Sender:
    def __init__(self):     
        self.sock = socket.socket(socket.AF_INET, # Internet
                         socket.SOCK_DGRAM) # UDP
    def send(self, data, adress = "127.0.0.1", port = 9900):
        self.sock.sendto(json.dumps(data).encode(), ("127.0.0.1",9900))