import pickle
import socket
import time
from functools import reduce

#searches for a document which contains all the words from the querie
def inverted_search(inverted, list):
    words = []
    gen = (word for word in list if word in inverted)
    for word in gen:
        words.append(word)

    results = [set(inverted[word].keys()) for word in words]
    return reduce(lambda x, y: x & y, results) if results else []

#load the inverted index
with open('inverted_index.pkl', 'rb') as f:
    inverted_index = pickle.load(f)

# print(inverted_index)
# queries = ['if']
# print(inverted_search(inverted_index, queries))

#connect to a c# server
host, port = "127.0.0.1", 25001
sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.connect((host, port))

#Exchange initial messages with the c# server
receivedData = sock.recv(1024).decode("UTF-16") #receiveing data in Byte fron C#, and converting it to String
print(receivedData)
time.sleep(1)
start = 'Python server ... [connected]'
sock.sendall(start.encode("UTF-16"))  # Converting string to Byte, and sending it to C#

#process the server requests and respond with the list of documents
while True:
    time.sleep(1)
    receivedData = sock.recv(1024).decode("UTF-16") #receiveing data in Byte fron C#, and converting it to String
    receivedData = list(receivedData.split(" "))
    print('<--', receivedData)
    answer = inverted_search(inverted_index, receivedData)
    answer = ' '.join(answer)
    print('-->', answer if answer else [])
    sock.sendall(answer.encode("UTF-16"))