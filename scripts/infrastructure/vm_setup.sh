#!/bin/bash

# Ubuntu 20.04LTS

mkdir /var/app

# dotnet core 3.1
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb

apt-get update
apt-get install -y aspnetcore-runtime-3.1
rm packages-microsoft-prod.deb

# postgres 12
wget --quiet -O - https://www.postgresql.org/media/keys/ACCC4CF8.asc | apt-key add -
echo "deb http://apt.postgresql.org/pub/repos/apt/ `lsb_release -cs`-pgdg main" | tee  /etc/apt/sources.list.d/pgdg.list
apt-get update
apt-get install -y postgresql-12

# nginx
apt-get install -y nginx

# certbot
snap install --classic certbot
ln -s /snap/bin/certbot /usr/bin/certbot
apt-get install acl

# firewall
ufw allow 'OpenSSH'
ufw allow 'Nginx HTTPS'

####
# Manual Setup
####

# Admin User
adduser <username>
usermod -aG sudo <username>
mkdir /home/<username>/.ssh
touch /home/<username>/.ssh/authorized_keys # <- put ssh public key here

# Disable Root
sudo vi /etc/ssh/sshd_config
# set -> PermitRootLogin no
# set -> AllowUsers <username> <ci_cd_username>

# Database Setup
passwd postgres
service postgresql start

createdb <db_name>
createuser --no-createdb --login --pwprompt <username>
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO <username>; # in <db_name>


# setup github actions account
adduser <ci_cd_username>
setfacl -R -m u:<ci_cd_username>:rwx /var/app
mkdir /home/<ci_cd_username>/.ssh
mkdir /home/<ci_cd_username>/.ssh/authorized_keys # <- put ssh public key here


# setup systemd service
# mv shop.service /etc/systemd/system/
sudo systemctl start shop

# setup nginx config after deploy
sudo nginx -t
sudo nginx -s reload

# setup ssl with cert bot
sudo certbot --nginx