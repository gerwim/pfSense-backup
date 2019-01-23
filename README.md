# pfSense backup (pfSense 2.3.3 and higher)
## What does this tool do?
This tool has been created for users who want to download pfSense configuration backups with ease and store the configuration files themselves.  
  
It's based on [https://doc.pfsense.org/index.php/Remote_Config_Backup](https://doc.pfsense.org/index.php/Remote_Config_Backup).
  
Big note: if you want to support pfSense, you might want to get a [gold subscription](https://portal.pfsense.org/members/signup/gold) which supports both the project and you'll get access to the [AutoConfigBackup](https://doc.pfsense.org/index.php/AutoConfigBackup) tool.

## How do I run this?
### Easy way:
`docker run --rm -v /my/backup/folder:/app/backups reg.gerwim.nl/os/pfsense-backup https://192.168.0.1:8443 admin password`  
  
This will store the backup in `/my/backup/folder`. Just add the command above in a cronjob, task scheduler or something else to create backups on a recurring base.

### Compile it yourself way:
1) Clone this git repository (and `cd` into it)  
2) Run `docker build -t pfsense-backup .`  
3) Run (basically) the same command as above: `docker run --rm -v /my/backup/folder:/app/backups pfsense-backup https://192.168.0.1:8443 admin password`

## Automatically backup multiple pfSense machines
1) Create the backup folders (`mkdir -p /opt/pfSense/backups/pfSense-{master,slave,another}`)  
2) Create a new script: `vi autoBackup.sh` with the contents below (modify where needed!) 
```
#!/bin/bash

BackupTarget() {
	docker run --rm -v $1:/app/backups reg.gerwim.nl/os/pfsense-backup $2 $3 $4 >> /opt/pfSense/output.log
}

# Backup master
BackupTarget "/opt/pfSense/backups/pfSense-master" "https://10.10.10.2:444" "admin" "password"

# Backup slave
BackupTarget "/opt/pfSense/backups/pfSense-slave" "https://10.10.10.3:444" "admin" "password"

# Backup another pfSense
BackupTarget "/opt/pfSense/backups/pfSense-another" "https://10.10.10.4:444" "admin" "password"
```

3) Finally make the script executable `chmod +x autoBackup.sh` and run it: `./autoBackup.sh`. You can run this file by cron to backup your pfSense systems automatically.
