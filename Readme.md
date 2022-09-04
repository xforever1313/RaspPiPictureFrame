# RaspPiPictureFrame

A digital picture frame for the Raspberry Pi 3.

This is a rewrite of [PiPictureFrame](https://github.com/xforever1313/PiPictureFrame) using modern versions of Dotnet.

## Install Instructions

### Dependencies to install

Download the following packages from your package manager after setting up a Raspberry Pi.

* pqiv (Required for rendering images to the screen).
* matchbox-keyboard (Optional: useful for touchscreen, however).
* xscreensaver (To prevent the screen from turning off).
  * After installing, go to the Menu button on the desktop -> Preferences -> Screensaver.  Under the "Display Modes: tab, and under the "Mode" drop down, select "Disable Screen Saver".  A reboot may be required.
* nginx (Optional:  Only needed if configuring reverse proxy).

### Autologin

This is how to auto-login to the desktop so the user can start the frame:

Edit /etc/lightdm/lightdm.conf

This should be there by default if on a Raspberry Pi, but if its not:

```txt
"autologin-user=userName"
```

### Security

Force key login for SSH:

Edit /etc/ssh/sshd_config

```txt
PermitRootLogin no
PasswordAuthentication no
Port XXXX  # Change port if desired
```

### Setting up Reverse Proxy (optional)

It is recommended to run RaspPiPictureFrame as the user that is automatically logged-in.  This usually is not the root user.  To connect to the RaspPiPictureFrame via a web browser, one could set a port and could connect to ```http://RaspPiPictureFrame:9001``` (this can be done by setting the --urls command line argument to ```--urls=http://*9001```).  However, if you do not want to specify a port, you need to have the RaspPiPictureFrame web server run on port 80.  Usually, only root is able to use port 80.  However, you can setup a reverse proxy to make it so you can access the web browser control while not running RaspPiPictureFrame as root.

* Install nginx with ```sudo apt install nginx```
* Enable the nginx daemon with ```sudo systemctl enable nginx```
* Run ```sudo nano /etc/nginx/sites-enabled/RaspPiPictureFrame``` to edit the nginx configuration.
* Paste in the below configuration:

  ```txt
  server {
    listen        80;
    server_name _;
    location / {
        proxy_pass         http://localhost:9001;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade $http_upgrade;
        proxy_set_header   Connection keep-alive;
        proxy_set_header   Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
    }
  }
  ```

* When starting RaspPiPictureFrame, ensure the --urls argument's port matches the port number above.  If using the above configuration, invoke ```--urls=http://localhost:9001```.

## Running

All non-user-configurable settings are set on the command line.  Since this is running ASP.Net Core, all of their command line arguments are also valid.  Here are the command-line arguments that are most important.

* ```--help``` - Prints the help message and exits.
* ```--print_license``` - Prints the license and exits.
* ```--print_readme``` - Prints this document and exits.
* ```--print_credits``` - Prints the third-party licenses and exits.
* ```--version``` - Prints the version and exits.
* ```--picture_directory=xxx``` - Sets the directory of where to find and upload pictures to xxx.  If not specified, this is defaulted the Pictures directory in your home directory.
* ```--urls=``` - This is an ASP.Net argument.  This setting says who is allowed to connect to the web interface, and the port number.  To allow all incoming connections on port 9001, for example, set this to ```--urls=http://*:9001```.  To restrict only internal connections on port 9001 (e.g. when using a reverse proxy), set this to ```--urls=http://localhost:9001```.  See Microsoft's documentation [here](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/web-host?view=aspnetcore-6.0#server-urls) for more information about this argument.
