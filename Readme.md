# RaspPiPictureFrame

A digital picture frame for the Raspberry Pi 3.

This is a rewrite of [PiPictureFrame](https://github.com/xforever1313/PiPictureFrame) using modern versions of Dotnet.

## Install Instructions

### Create a Raspberry Pi Image

TODO

### Dependencies to install

Download the following packages from your package manager after setting up a Raspberry Pi.

* dotnet-runtime-6.0
* matchbox-keyboard (Optional: useful for touchscreen, however)
* xscreensaver (To prevent the screen from turning off)
* pqiv (For rendering images to the screen)

### Autologin

This is how to auto-login to the desktop so the user can start the frame:

Edit /etc/lightdm/lightdm.conf

This should be there by default, but if its not:

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
