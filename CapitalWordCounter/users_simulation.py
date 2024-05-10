import os
# If requests are not recognized use: py -m pip install requests
# (this command is for Windows; previously installed pip is also required;)
import requests
import random
import threading
import time

# Function to get all text file names from the directory and its subdirectories
def get_text_files(directory):
    txt_files = []
    for root, dirs, files in os.walk(directory):
        for file in files:
            if file.endswith(".txt"):
                txt_files.append(file)  # Appends only the filename
    return txt_files

# URL to access, change it if needed
url = "http://localhost:18859/"

# Function to get random filename to use with the URL
def get_random_filename():
    return random.choice(txt_files)

# Function to simulate user behavior
def simulate_user(thread_name):
    while not stop_event.is_set():
        filename = get_random_filename()
        # Introduced randomness in request frequency
        time.sleep(random.uniform(0.1, 2))
        print(f"User {thread_name} trying to enter: {url + filename}")
        response = requests.get(url + filename)
        if response.status_code == 200:
            print(f"User {thread_name} accessed: {url + filename}")
        else:
            print(f"User {thread_name} got an error when accessing: {url + filename}")
        # Introduced randomness in request frequency
        time.sleep(random.uniform(0.1, 4))

# Function to start specified number of user threads
def start_users(num_users):
    threads = []
    for i in range(num_users):
        thread_name = f"Echo{i+1}"
        t = threading.Thread(target=simulate_user, args=(thread_name,))
        threads.append(t)
        t.start()

    # Simulate user activity for 20 seconds
    time.sleep(20)

    # Set the stop event to stop all user threads
    stop_event.set()

    # Wait for all threads to finish
    for t in threads:
        t.join()

if __name__ == "__main__":
    stop_event = threading.Event()
    directory = os.path.dirname(os.path.realpath(__file__))  # Get current directory
    txt_files = get_text_files(directory)
    num_users = int(input("Enter the number of users: "))
    start_users(num_users)
