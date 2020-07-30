# Creating a Server
You can use the scripts in this folder to create a VM that will run the programs. Make sure you don't have any src, data or secret folders in your clone and then run SetupAll.ps1.

# Create SSH Key
Run `ssh-keygen` to create a key. If this is created without a password login is automatic. If you do this throw it away at the end.

# Remove Sudo Password Prompt
1. Connect to the server `ssh threax@UbuntuTestServer`.
1. Run `sudo visudo`
1. At the bottom of the file add `YOUR_USERNAME_HERE ALL=(ALL) NOPASSWD: ALL`. The username is the one you want to use to sudo without a password.
1. Ctrl+x to save and close.

Scripts will run without prompts once you do these things.

# Setting Up Remote Server
This will create a new server setup on a remote server. This can be used on a totally blank Ubuntu server and all prereqs will be installed.

If you want to setup containers on a new remote Ubuntu server do the following:
1. Create the new server with ssh installed. Unlock sudo if wanted. This is ideal or you will enter the password a lot.
1. Create certs in the cert folder.
1. Run `ssh-keygen` if you haven't already.
1. Run `RemoteSetupAll.ps1`.
1. Create an account and enter the guid when prompted.

# Setting Up Local Server
The local setup assumes you already have docker and powershell installed.

If you want to run the containers on the current box do the following:
1. Create certs in the cert folder.
1. Run `LocalSetupAll.ps1`.
1. Create an account and enter the guid when prompted.