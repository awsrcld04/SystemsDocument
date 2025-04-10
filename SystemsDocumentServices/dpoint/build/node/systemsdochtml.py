import generalutils
import os
import re
import json
import sys

# write header initial html
def writeinitialhtml(scandatetimestring):
    tmphtmlstring = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\" \"http://www.w3.org/TR/html4/loose.dtd\">\n" \
    + '<html>\n' \
    + '<head>\n' \
    + '<title>SystemsDocument Documentation</title>\n' \
    + '<link rel=\"stylesheet\" type=\"text/css\" href=\"style.css\" />' \
    + '</head>\n' \
    + '<body>\n' \
    + '<p><b>Systems Documentation</b><br>\n' \
    + 'DocumentationFormat F0 v1.0</p>' \
    + '<p><b>Date/Time of Configuration Scan: ' + scandatetimestring + '</b></p>\n'

    return tmphtmlstring

# write ComputerSystem html
def writecomputersystemhtml(computersystem):
    fmtd_totalphysicalmemory = \
        generalutils.formatnumber(computersystem['totalphysicalmemory'])
    tmphtmlstring = \
    '<p><b>ServerName: ' + computersystem['servername'] + '</b></p>\n' \
    + '<p>Domain: ' + computersystem['domain'] + '</p>\n' \
    + '<p>Total Physical Memory: ' + fmtd_totalphysicalmemory + '</p>\n' \
    + '<p>System Type: ' + computersystem['systemtype'] + '</p>\n' \
    + '<p>Number of processors: ' + computersystem['numberofprocessors'] + '</p>\n' 

    return tmphtmlstring

# write Processor html
def writeprocessorshtml(processors):
    if processors is not None and len(processors) > 0:
        tmphtmlstring = '<p>Processor: ' + processors[0]['cpu'] + '</p>\n'

        return tmphtmlstring

# write Operating System html
def writeoperatingsystemhtml(operatingsystem):
    fmtd_osinstalldate = operatingsystem['osinstalldate'][4:6] + '/' + operatingsystem['osinstalldate'][6:8] + '/' \
    + operatingsystem['osinstalldate'][0:4]
    tmphtmlstring = \
    '<div id=\"operatingsystem\">\n<p><b>Operating System:</b></p>\n<div id=\"oselements\">\n' \
    + '<p>Operating System: ' + operatingsystem['osproduct'] + '</p>\n' \
    + '<p>Service Pack: ' + operatingsystem['osservicepack'] + '</p>\n' \
    + '<p>Version Number: ' + operatingsystem['osversionnumber'] + '</p>\n' \
    + '<p>Build: ' + operatingsystem['buildnumber'] + '</p>\n' \
    + '<p>Boot Device: ' + operatingsystem['bootdevice'] + '</p>\n' \
    + '<p>System Device: ' + operatingsystem['systemdevice'] + '</p>\n' \
    + '<p>System Directory: ' + operatingsystem['systemdirectory'] + '</p>\n' \
    + '<p>Windows Directory: ' + operatingsystem['windowsdirectory'] + '</p>\n' \
    + '<p>Install Date: ' + fmtd_osinstalldate + '</p>\n' \
    + '</div>\n</div>\n'

    return tmphtmlstring

# write SystemProduct html
def writesystemproducthtml(systemproduct):
    if systemproduct is not None:
        tmphtmlstring = \
        '<div id=\"operatingsystem\">\n<p><b>System:</b></p>\n<div id=\"sectionelements\">\n' \
        + '<p>Name: ' + systemproduct['systemname'] + '</p>\n' \
        + '<p>Version: ' + systemproduct['systemversion'] + '</p>\n' \
        + '</div>\n</div>\n'

        return tmphtmlstring

# write Hardware/BIOS html
def writehardwarebioshtml(hardwarebios):
    if hardwarebios is not None:
        tmphtmlstring = \
        '<div id=\"hardware\">\n<p><b>Hardware:</b></p>\n<div id=\"sectionelements\">\n' \
        + '<p>Manufacturer: ' + hardwarebios['hardwaremanufacturer'] + '</p>\n' \
        + '<p>Version: ' + hardwarebios['hardwareversion'] + '</p>\n' \
        + '<p>Serial number: ' + hardwarebios['hardwareserialnumber'] + '</p>\n' \
        + '</div>\n</div>\n'
    
        return tmphtmlstring

# write Volumes html
def writevolumeshtml(volumes):
    if volumes is not None:
        tmphtmlstring = \
        '<div id=\"operatingsystem\">\n<p><b>Volumes: ' + str(generalutils.count(volumes)) + '</b></p>\n<div id=\"sectionelements\">\n'
        bgflag = 'no'
        for volume in volumes:
            if bgflag == 'no':
                tmphtmlstring = tmphtmlstring + '<div style=\"background-color: #ffffff;\">\n'
            else:
                tmphtmlstring = tmphtmlstring + '<div style=\"background-color: #C0C0C0;\">\n'
            tmphtmlstring = tmphtmlstring \
            + '<p>Volume Caption: ' + volume['volcaption'] + '</p>\n' \
            + '<p>Volume Name: ' + volume['volname'] + '</p>\n' \
            + '<p>Volume Drive Letter: ' + volume['voldriveletter'] + '</p>\n' \
            + '<p>Volume Label: ' + volume['vollabel'] + '</p>\n'
            tmphtmlstring = tmphtmlstring + '</div>\n'
            if bgflag == 'no':
                bgflag = 'yes'
            else:
                bgflag = 'no'        
        tmphtmlstring = tmphtmlstring + '</div>\n</div>\n'

        return tmphtmlstring 

# write Disks html
def writediskshtml(disks):
    drivetypes = { '2' : 'Removable media', '3' : 'Local disk', \
                   '4' : 'Network drive', '5' : 'CD/DVD', \
                   '6' : 'RAM disk' }
    
    tmphtmlstring = '<div id=\"operatingsystem\">\n<p><b>Disks: ' + str(generalutils.count(disks)) + '</b></p>\n<div id=\"sectionelements\">\n'
    bgflag = 'no' 
    for x in disks:
        if bgflag == 'no':
            tmphtmlstring = tmphtmlstring + '<div style=\"background-color: #ffffff;\">\n'
        else:
            tmphtmlstring = tmphtmlstring + '<div style=\"background-color: #C0C0C0;\">\n'
        if x['drivetype'] in ['3']:
            fmtd_size = generalutils.formatnumber(x['drivesize'])
            fmtd_freespace = generalutils.formatnumber(x['drivefreespace'])
            tmpdrvsize = long(x['drivesize'])
            tmpdrvfree = long(x['drivefreespace'])       
            tmpdrvused = tmpdrvsize - tmpdrvfree
            tmpstrused = str(tmpdrvused)
            fmtd_usedspace = generalutils.formatnumber(tmpstrused)
            tmphtmlstring = tmphtmlstring \
            + '<p>Drive Letter: ' + x['driveletter'] + '</p>\n' \
            + '<p>Drive/Volume Label: ' + x['drivelabel'] + '</p>\n' \
            + '<p>Drive Size: ' + fmtd_size + '</p>\n' \
            + '<p>Drive Free Space: ' + fmtd_freespace + '</p>\n' \
            + '<p>Drive Used Space: ' + fmtd_usedspace + '</p>\n' \
            + '<p>Drive Type: ' + drivetypes[x['drivetype']] + '</p>\n' \
            + '<p>Drive Filesystem: ' + x['drivefilesystem'] + '</p>\n'
        if x['drivetype'] in ['2','4','5','6']:
            tmpdrivelabel = x['drivelabel']
            if tmpdrivelabel == '<na>':
                tmpdrivelabel = ' '
            tmpdrivefilesystem = x['drivefilesystem']
            if tmpdrivefilesystem == '<na>':
                tmpdrivefilesystem = ' '
            tmphtmlstring = tmphtmlstring \
            + '<p>Drive Letter: ' + x['driveletter'] + '</p>\n' \
            + '<p>Drive/Volume Label: ' + tmpdrivelabel + '</p>\n' \
            + '<p>Drive Type: ' + drivetypes[x['drivetype']] + '</p>\n' \
            + '<p>Drive Filesystem: ' + tmpdrivefilesystem + '</p>\n'
        tmphtmlstring = tmphtmlstring + '</div>\n'
        if bgflag == 'no':
            bgflag = 'yes'
        else:
            bgflag = 'no'
    tmphtmlstring = tmphtmlstring + '</div>\n</div>\n'
    
    return tmphtmlstring  

# write DriveRoots html
def writedriverootshtml(driveroots):  
    tmphtmlstring = '<div id=\"operatingsystem\">\n<p><b>Drive Root Directories: </b></p>\n<div id=\"sectionelements\">\n'
    bgflag = 'no' 
    for x in driveroots:
        if x['DriveLetter'] is not None and x['DriveRootFolders'] is not None:
            if bgflag == 'no':
                tmphtmlstring = tmphtmlstring + '<div style=\"background-color: #ffffff;\">\n'
            else:
                tmphtmlstring = tmphtmlstring + '<div style=\"background-color: #C0C0C0;\">\n'

            tmphtmlstring = tmphtmlstring + '<p>Drive Root: ' + x['DriveLetter'] + '</p>\n'      
            rootfolders = x['DriveRootFolders']
            if rootfolders is not None:
                for y in rootfolders:
                    tmphtmlstring = tmphtmlstring + '<p>Root Folder: ' + y + '</p>\n'
            tmphtmlstring = tmphtmlstring + '</div>\n'
            if bgflag == 'no':
                bgflag = 'yes'
            else:
                bgflag = 'no'
    tmphtmlstring = tmphtmlstring + '</div>\n</div>\n'
    
    return tmphtmlstring

# write ProgramFilesRoots html
def writeprogramfilesrootshtml(programfilesroots):  
    tmphtmlstring = '<div id=\"operatingsystem\">\n<p><b>Program Files Directories: </b></p>\n<div id=\"sectionelements\">\n'
    bgflag = 'no' 
    for x in programfilesroots:
        if bgflag == 'no':
            tmphtmlstring = tmphtmlstring + '<div style=\"background-color: #ffffff;\">\n'
        else:
            tmphtmlstring = tmphtmlstring + '<div style=\"background-color: #C0C0C0;\">\n'

        tmphtmlstring = tmphtmlstring + '<p>Program Files Path: ' + x['ProgramFilesPath'] + '</p>\n'      
        folders = x['ProgramFilesFolders']
        if folders is not None:
            for y in folders:
                tmphtmlstring = tmphtmlstring + '<p>SubDirectory: ' + y + '</p>\n'
        tmphtmlstring = tmphtmlstring + '</div>\n'
        if bgflag == 'no':
            bgflag = 'yes'
        else:

            bgflag = 'no'
    tmphtmlstring = tmphtmlstring + '</div>\n</div>\n'
    
    return tmphtmlstring

# write NIC html
def writenichtml(netadapterconfigs):
    if netadapterconfigs is not None:
        tmphtmlstring = '<div id=\"hardware\">\n<p><b>Networking: </b></p>\n<div id=\"sectionelements\">\n'
        bgflag = 'no'
        for nic in netadapterconfigs:
            if nic['nicconfigipaddress'] is not None and nic['nicconfigipaddress'] != '' and \
            nic['nicconfigipaddress'] != '<na>':
                if bgflag == 'no':
                    tmphtmlstring = tmphtmlstring + '<div style=\"background-color: #ffffff;\">\n'
                else:
                    tmphtmlstring = tmphtmlstring + '<div style=\"background-color: #C0C0C0;\">\n'
                tmphtmlstring = tmphtmlstring \
                + '<p>Network Adapter Label: ' + nic['nicconfiglabel'][11:len(nic['nicconfiglabel'])] + '</p>\n' \
                + '<p>Network Adapter IP Address(es): ' +  nic['nicconfigipaddress']  + '</p>\n' \
                + '<p>Network Adapter IP Subnet Mask(s): ' +  nic['nicconfigipsubnet']  + '</p>\n' \
                + '<p>Network Adapter Default IP Gateway: ' +  nic['nicconfigdefaultipgateway']  + '</p>\n' \
                + '<p>Network Adapter MAC Address: ' +  nic['nicconfigmacaddress']  + '</p>\n'
                tmphtmlstring = tmphtmlstring + '</div>\n'
                if bgflag == 'no':
                    bgflag = 'yes'
                else:
                    bgflag = 'no'    
        tmphtmlstring = tmphtmlstring + '</div>\n</div>\n'

        return tmphtmlstring

# write Shares html
def writeshareshtml(shares):
    sharetypes = { '0' : 'Disk Drive', '1' : 'Print Queue', \
                   '2' : 'Device', '3' : 'IPC', \
                   '2147483648' : 'Disk Drive Admin', '2147483649' : 'Print Queue Admin', \
                   '2147483650' : 'Device Admin', '2147483651' : 'IPC Admin' }
    if shares is not None:
        tmphtmlstring = \
        '<div id=\"hardware\">\n<p><b>Shares: ' + str(generalutils.count(shares))+'</b></p>\n<div id=\"sectionelements\">\n'
        bgflag = 'no'
        for share in shares:
            if bgflag == 'no':
                tmphtmlstring = tmphtmlstring + '<div style=\"background-color: #ffffff;\">\n'
            else:
                tmphtmlstring = tmphtmlstring + '<div style=\"background-color: #C0C0C0;\">\n'
            tmphtmlstring = tmphtmlstring \
            + '<p>Share Name: ' + share['sharename'] + '</p>\n' \
            + '<p>Share Label: ' + share['sharelabel'] + '</p>\n' \
            + '<p>Share Description: ' + share['sharedescription'] + '</p>\n' \
            + '<p>Share Path: ' + share['sharepath'] + '</p>\n' \
            + '<p>Share Type: ' + sharetypes[share['sharetype']] + '</p>\n'
            tmphtmlstring = tmphtmlstring + '</div>\n'
            if bgflag == 'no':
                bgflag = 'yes'
            else:
                bgflag = 'no' 
        tmphtmlstring = tmphtmlstring + '</div>\n</div>\n'

        return tmphtmlstring

# write Roles and Features html
def writerolesandfeatureshtml(rolesandfeatures):
    if rolesandfeatures is not None:
        tmphtmlstring = \
        '<div id=\"hardware\">\n<p><b>Roles and Features: ' + str(generalutils.count(rolesandfeatures))+'</b></p>\n<div id=\"sectionelements\">\n'
        raf = []
        for x in rolesandfeatures:
            raf.append(x['roleorfeaturename'])
        raf.sort()
        bgflag = 'no' 
        for item in raf:
            if bgflag == 'no':
                tmphtmlstring = tmphtmlstring + '<div style=\"background-color: #ffffff;\">\n'
            else:
                tmphtmlstring = tmphtmlstring + '<div style=\"background-color: #C0C0C0;\">\n'
            tmphtmlstring = tmphtmlstring + '<p>Role/Feature: ' + item + '</p>\n'      

            tmphtmlstring = tmphtmlstring + '</div>\n'
            if bgflag == 'no':
                bgflag = 'yes'
            else:
                bgflag = 'no'
        tmphtmlstring = tmphtmlstring + '</div>\n</div>\n'

        return tmphtmlstring

# write Share Permissions html
def writesharepermissionshtml(sharepermissions):
    if sharepermissions is not None:
        tmphtmlstring = \
        '<div id=\"hardware\">\n<p><b>Share Permissions: </b></p>\n<div id=\"sectionelements\">\n'
        bgflag = 'no' 
        for item in sharepermissions:
            if bgflag == 'no':
                tmphtmlstring = tmphtmlstring + '<div style=\"background-color: #ffffff;\">\n'
            else:
                tmphtmlstring = tmphtmlstring + '<div style=\"background-color: #C0C0C0;\">\n'
            tmphtmlstring = tmphtmlstring + '<p>Share: ' + item.split(':')[0] + '</p>\n' \
            + '<p>Group|User: ' + item.split(':')[1] + '</p>\n' \
            + '<p>Permission: ' + item.split(':')[2] + '</p>\n'
            tmphtmlstring = tmphtmlstring + '</div>\n'
            if bgflag == 'no':
                bgflag = 'yes'
            else:
                bgflag = 'no'
        tmphtmlstring = tmphtmlstring + '</div>\n</div>\n'

        return tmphtmlstring

# Printers
def writeprintershtml(printers):
    if printers is not None:
        tmphtmlstring = \
        '<div id=\"hardware\">\n<p><b>Printers: ' + str(generalutils.count(printers)) +'</b></p>\n<div id=\"sectionelements\">\n'
        bgflag = 'no' 
        for item in printers:
            if bgflag == 'no':
                tmphtmlstring = tmphtmlstring + '<div style=\"background-color: #ffffff;\">\n'
            else:
                tmphtmlstring = tmphtmlstring + '<div style=\"background-color: #C0C0C0;\">\n'
            prtdescr = item['printerdescription']
            if prtdescr == '<na>':
                prtdescr = ''
            tmphtmlstring = tmphtmlstring + '<p>Printer Name: ' + item['printername'] + '</p>\n' \
            + '<p>Printer Description: ' + prtdescr + '</p>\n' \
            + '<p>Printer DriverName: ' + item['drivername'] + '</p>\n' \
            + '<p>Printer PortName: ' + item['portname'] + '</p>\n' 
            tmphtmlstring = tmphtmlstring + '</div>\n'
            if bgflag == 'no':
                bgflag = 'yes'
            else:
                bgflag = 'no'
        tmphtmlstring = tmphtmlstring + '</div>\n</div>\n'

        return tmphtmlstring

# ODBC
def writeodbchtml(datasources):
    if datasources is not None:
        tmphtmlstring = \
        '<div id=\"hardware\">\n<p><b>ODBC Data Source(s): ' + str(generalutils.count(datasources)) +'</b></p>\n<div id=\"sectionelements\">\n'
        bgflag = 'no' 
        for item in datasources:
            if bgflag == 'no':
                tmphtmlstring = tmphtmlstring + '<div style=\"background-color: #ffffff;\">\n'
            else:
                tmphtmlstring = tmphtmlstring + '<div style=\"background-color: #C0C0C0;\">\n'
            tmphtmlstring = tmphtmlstring + '<p>Data Source: ' + item['DataSource'] + '</p>\n' \
            + '<p>Driver: ' + item['Driver'] + '</p>\n'
            tmphtmlstring = tmphtmlstring + '</div>\n'
            if bgflag == 'no':
                bgflag = 'yes'
            else:
                bgflag = 'no'
        tmphtmlstring = tmphtmlstring + '</div>\n</div>\n'

        return tmphtmlstring

# Products
def writeproductshtml(products):
    if products is not None:
        tmphtmlstring = \
        '<div id=\"hardware\">\n<p><b>Products: ' + str(generalutils.count(products)) +'</b></p>\n<div id=\"sectionelements\">\n'
        bgflag = 'no'
        for product in products:
            if bgflag == 'no':
                tmphtmlstring = tmphtmlstring + '<div style=\"background-color: #ffffff;\">\n'
            else:
                tmphtmlstring = tmphtmlstring + '<div style=\"background-color: #C0C0C0;\">\n'

            tmphtmlstring = tmphtmlstring + '<p>Product Name: ' + product + '</p>\n'

            tmphtmlstring = tmphtmlstring + '</div>\n'
            if bgflag == 'no':
                bgflag = 'yes'
            else:
                bgflag = 'no'
        tmphtmlstring = tmphtmlstring + '</div>\n</div>\n'

        return tmphtmlstring

# write User Accounts html
def writeuseraccountshtml(uaaccounts):
    excludeduseraccounts = ['\\Guest', '\\krbtgt', '\\SUPPORT_388945a0']
    if uaaccounts is not None:
        nonexcludeduseraccounts = []
        for uaaccount in uaaccounts:
            tmpuseraccountname = '\\' + uaaccount['uacaption'].split('\\')[1]
            if tmpuseraccountname not in excludeduseraccounts:
                nonexcludeduseraccounts.append(uaaccount)
        tmphtmlstring = \
        '<div id=\"hardware\">\n<p><b>User Accounts: ' + str(len(nonexcludeduseraccounts)) + '</b></p>\n<div id=\"sectionelements\">\n'
        bgflag = 'no'
        if len(nonexcludeduseraccounts) > 0:
            for ne_uaaccount in nonexcludeduseraccounts:
                if bgflag == 'no':
                    tmphtmlstring = tmphtmlstring + '<div style=\"background-color: #ffffff;\">\n'
                else:
                    tmphtmlstring = tmphtmlstring + '<div style=\"background-color: #C0C0C0;\">\n'
                tmphtmlstring = tmphtmlstring \
                + '<p>User Account Domain|Server FullName: ' + ne_uaaccount['uacaption'] + '</p>\n' \
                + '<p>User Account Domain: ' + ne_uaaccount['uadomain'] + '</p>\n' \
                + '<p>User Account Name: ' + ne_uaaccount['uaname'] + '</p>\n' \
                + '<p>User Account FullName: ' + ne_uaaccount['uafullname'] + '</p>\n' \
                + '<p>User Account Description: ' + ne_uaaccount['uadescription'] + '</p>\n'
                tmphtmlstring = tmphtmlstring + '</div>\n'
                if bgflag == 'no':
                    bgflag = 'yes'
                else:
                    bgflag = 'no' 
        tmphtmlstring = tmphtmlstring + '</div>\n</div>\n'

        return tmphtmlstring

# write Groups html
def writegroupshtml(groups):
    excludedgroups = [ \
    '\\Account Operators' , \
    '\\Administrators' , \
    '\\Allowed RODC Password Replication Group' , \
    '\\Backup Operators' , \
    '\\Cert Publishers' , \
    '\\Certificate Service DCOM Access' , \
    '\\Cryptographic Operators' , \
    '\\Denied RODC Password Replication Group' , \
    '\\Distributed COM Users' , \
    '\\Domain Admins' , \
    '\\Domain Computers' , \
    '\\Domain Controllers' , \
    '\\Domain Guests' , \
    '\\Domain Users' , \
    '\\DnsAdmins' , \
    '\\DnsUpdateProxy' , \
    '\\Enterprise Admins' , \
    '\\Enterprise Read-only Domain Controllers' , \
    '\\Event Log Readers' , \
    '\\Group Policy Creator Owners' , \
    '\\Guests' , \
    '\\HelpServicesGroup', \
    '\\IIS_IUSRS' , \
    '\\Incoming Forest Trust Builders' , \
    '\\Network Configuration Operators' , \
    '\\Performance Log Users' , \
    '\\Performance Monitor Users' , \
    '\\Power Users', \
    '\\Pre-Windows 2000 Compatible Access' , \
    '\\Print Operators' , \
    '\\RAS and IAS Servers' , \
    '\\Read-only Domain Controllers' , \
    '\\Remote Desktop Users' , \
    '\\Replicator' , \
    '\\Schema Admins' , \
    '\\Server Operators' , \
    '\\TelnetClients', \
    '\\Terminal Server License Servers' , \
    '\\Users' , \
    '\\Windows Authorization Access Group']
    if groups is not None:
        nonexcludedgroups = []
        for group in groups:
            tmpgroupname = '\\' + group['grpcaption'].split('\\')[1]
            if tmpgroupname not in excludedgroups:
                nonexcludedgroups.append(group)
        tmphtmlstring = \
        '<div id=\"hardware\">\n<p><b>Groups: ' + str(len(nonexcludedgroups)) + '</b></p>\n<div id=\"sectionelements\">\n'
        bgflag = 'no'
        if len(nonexcludedgroups) > 0:
            for ne_group in nonexcludedgroups:
                if bgflag == 'no':
                    tmphtmlstring = tmphtmlstring + '<div style=\"background-color: #ffffff;\">\n'
                else:
                    tmphtmlstring = tmphtmlstring + '<div style=\"background-color: #C0C0C0;\">\n'
                tmphtmlstring = tmphtmlstring \
                + '<p>Group Domain|Server FullName: ' + ne_group['grpcaption'] + '</p>\n' \
                + '<p>Group Domain: ' + ne_group['grpdomain'] + '</p>\n' \
                + '<p>Group Name: ' + ne_group['grpname'] + '</p>\n' \
                + '<p>Group Description: ' + ne_group['grpdescription'] + '</p>\n'
                tmphtmlstring = tmphtmlstring + '</div>\n'
                if bgflag == 'no':
                    bgflag = 'yes'
                else:
                    bgflag = 'no'
        tmphtmlstring = tmphtmlstring + '</div>\n</div>\n'

        return tmphtmlstring

# write Service Accounts html
def writeserviceaccountshtml(services):
    systemaccounts = ['LocalSystem', 'localSystem', 'NT AUTHORITY\\LocalService', 'NT Authority\\LocalService', \
        'NT Authority\\NetworkService', 'NT AUTHORITY\\NetworkService', 'NT AUTHORITY\\NETWORKSERVICE', \
        'NT AUTHORITY\\LOCALSERVICE']
    serviceaccountlist = []
    for service in services:
        if 'NT Service\\' not in service['servicestartname'] and not service['servicestartname'] in systemaccounts:
            serviceaccountlist.append(service['servicestartname'])   
    tmphtmlstring = '<div id=\"hardware\">\n<p><b>Service Accounts: ' +  str(len(serviceaccountlist)) \
        + '</b></p>\n<div id=\"sectionelements\">\n'
    if len(serviceaccountlist) > 0:
        bgflag = 'no'
        for account in serviceaccountlist:
            if bgflag == 'no':
                tmphtmlstring = tmphtmlstring + '<div style=\"background-color: #ffffff;\">\n'
            else:
                tmphtmlstring = tmphtmlstring + '<div style=\"background-color: #C0C0C0;\">\n'
            tmphtmlstring = tmphtmlstring + '<p>Service Account: ' + account + '</p>\n'
            tmphtmlstring = tmphtmlstring + '</div>\n'
            if bgflag == 'no':
                bgflag = 'yes'
            else:
                bgflag = 'no'       
    tmphtmlstring = tmphtmlstring + '</div>\n</div>\n'

    return tmphtmlstring

# write Services html
def writeserviceshtml(services):
    tmphtmlstring = '<div id=\"hardware\">\n<p><b>Services:</b></p>\n<div id=\"sectionelements\">\n'
    bgflag = 'no'
    for service in services:
        if bgflag == 'no':
            tmphtmlstring = tmphtmlstring + '<div style=\"background-color: #ffffff;\">\n'
        else:
            tmphtmlstring = tmphtmlstring + '<div style=\"background-color: #C0C0C0;\">\n'
        tmphtmlstring = tmphtmlstring \
        + '<p>Service Name: ' + service['servicename'] + '</p>\n' \
        + '<p>Service Path Name: ' + service['servicepathname'] + '</p>\n' \
        + '<p>Service State: ' + service['servicestate'] + '</p>\n' \
        + '<p>Service Start Mode: ' + service['servicestartmode'] + '</p>\n' \
        + '<p>Service Credentials: ' + service['servicestartname'] + '</p>\n'
        tmphtmlstring = tmphtmlstring + '</div>\n'
        if bgflag == 'no':
            bgflag = 'yes'
        else:
            bgflag = 'no'       
    tmphtmlstring = tmphtmlstring + '</div>\n</div>\n'

    return tmphtmlstring

# write Appendix
def writeappendixhtml():
    # the last item in each section needs two <br> tags
    tmphtmlstring = '<br><hr /><p><strong>Appendix</strong><br><br>'
    tmphtmlstring = tmphtmlstring + '<strong>Groups:</strong><br>1) The following system or built-in groups are excluded from the document:<br><br>' + \
    'Account Operators<br>' + \
    'Administrators<br>' + \
    'Allowed RODC Password Replication Group<br>' + \
    'Backup Operators<br>' + \
    'Cert Publishers<br>' + \
    'Certificate Service DCOM Access<br>' + \
    'Cryptographic Operators<br>' + \
    'Denied RODC Password Replication Group<br>' + \
    'Distributed COM Users<br>' + \
    'Domain Admins<br>' + \
    'Domain Computers<br>' + \
    'Domain Controllers<br>' + \
    'Domain Guests<br>' + \
    'Domain Users<br>' + \
    'DnsAdmins<br>' + \
    'DnsUpdateProxy<br>' + \
    'Enterprise Admins<br>' + \
    'Enterprise Read-only Domain Controllers<br>' + \
    'Event Log Readers<br>' + \
    'Group Policy Creator Owners<br>' + \
    'Guests<br>' + \
    'HelpServicesGroup<br>' + \
    'IIS_IUSRS<br>' + \
    'Incoming Forest Trust Builders<br>' + \
    'Network Configuration Operators<br>' + \
    'Performance Log Users<br>' + \
    'Performance Monitor Users<br>' + \
    'Power Users<br>' + \
    'Pre-Windows 2000 Compatible Access<br>' + \
    'Print Operators<br>' + \
    'RAS and IAS Servers<br>' + \
    'Read-only Domain Controllers<br>' + \
    'Remote Desktop Users<br>' + \
    'Replicator<br>' + \
    'Schema Admins<br>' + \
    'Server Operations<br>' + \
    'TelnetClients<br>' + \
    'Terminal Server License Servers<br>' + \
    'Users<br>' + \
    'Windows Authorization Access Group<br><br>'
    tmphtmlstring = tmphtmlstring + '<strong>Products:</strong><br>1) All products that may be listed in this document under the Products section may not show up in the Programs list from the Control Panel. The installer or install software for products installed on the system may not function identically and may install other products/components during installation.<br><br>'
    tmphtmlstring = tmphtmlstring + '<strong>ODBC:</strong><br>1) All ODBC Data Sources that may be listed in this document under the ODBC section are system data sources only.<br><br>'
    tmphtmlstring = tmphtmlstring + '<strong>Share Permissions:</strong><br>1) Permissions for the following shares are excluded from the document:<br><br>' + \
    'ADMIN$<br>' + \
    'Drive admin shares (C$)<br>' + \
    'IPC$<br>' + \
    'NETLOGON<br>' + \
    'print$<br>' + \
    'SYSVOL<br><br>'
    tmphtmlstring = tmphtmlstring + '<strong>User Accounts:</strong><br>1) The following system or built-in user accounts are excluded from the document:<br><br>' + \
    'Guest<br>' + \
    'krbtgt<br>' + \
    'SUPPORT_388945a0<br><br>'
    tmphtmlstring = tmphtmlstring + '<strong>Additional information:</strong><br>1) The lists (Groups, Shares under Share Permissions, User Accounts) within the Appendix may be modified in the future as necessary in future documents.'

    tmphtmlstring = tmphtmlstring + '</p>'

    return tmphtmlstring

# write ending html
def writeendinghtml():
    tmphtmlstring = '</body>\n</html>\n'

    return tmphtmlstring

# main method to generate documentation
def parsejsontohtml(inputfile_f, server):
    f2 = open(inputfile_f, 'w')

    tmpscandatetime = server['strConfigScanDateTime']
    fmtd_scandatetime = tmpscandatetime[0:2] + '/' + tmpscandatetime[2:4] + '/' \
    + tmpscandatetime[4:8] + ' ' + tmpscandatetime[8:10] + ':' + tmpscandatetime[10:12] \
    + ':' + tmpscandatetime[12:14]

    newhtml = writeinitialhtml(fmtd_scandatetime)

    # ComputerSystem
    tmpcomputersystem = server['ComputerSystem']

    newhtml = newhtml + writecomputersystemhtml(tmpcomputersystem)   

    # Processor
    tmpprocessors = server['Processors']

    newhtml = newhtml + writeprocessorshtml(tmpprocessors)

    # Operating System
    tmpoperatingsystem = server['OperatingSystem']

    newhtml = newhtml + writeoperatingsystemhtml(tmpoperatingsystem)

    # SystemProduct
    tmpsystemproduct = server['ComputerSystemProduct']

    newhtml = newhtml + writesystemproducthtml(tmpsystemproduct)

    # Hardware/BIOS
    tmphardwarebios = server['BIOS']

    newhtml = newhtml + writehardwarebioshtml(tmphardwarebios)

    # Volumes
    tmpvolumes = server['Volumes']

    if tmpvolumes is not None:
        newhtml = newhtml + writevolumeshtml(tmpvolumes)

    # Disks
    tmpdisks = server['Disks']

    newhtml = newhtml + writediskshtml(tmpdisks)

    # DriveRoots
    tmpdriveroots = server['DriveRoots']

    if tmpdriveroots is not None:
        newhtml = newhtml + writedriverootshtml(tmpdriveroots)

    # ProgramFilesRoots
    tmpprogramfilesroots = server['ProgramFilesRoots']

    if tmpprogramfilesroots is not None:
        newhtml = newhtml + writeprogramfilesrootshtml(tmpprogramfilesroots)

    # NIC
    tmpnetadapterconfigs = server['NetworkAdapterConfigurations']

    newhtml = newhtml + writenichtml(tmpnetadapterconfigs)

    # Roles and Features
    tmprolesandfeatures = server['ServerFeatures']

    if tmprolesandfeatures is not None:
        newhtml = newhtml + writerolesandfeatureshtml(tmprolesandfeatures)

    # Products
    tmpproducts = server['Products']

    if tmpproducts is not None:
        newhtml = newhtml + writeproductshtml(tmpproducts)

    #ODBC Data Sources
    tmpdatasources = server['SystemDataSources']

    if tmpdatasources is not None:
        newhtml = newhtml + writeodbchtml(tmpdatasources)

    # Shares
    tmpshares = server['Shares']

    if tmpshares is not None:
        newhtml = newhtml + writeshareshtml(tmpshares)

    # Share Permissions
    tmpsharepermissions = server['SharePermissions']

    if tmpsharepermissions is not None and len(tmpsharepermissions) > 0:
        newhtml = newhtml + writesharepermissionshtml(tmpsharepermissions)

    # Printers
    tmpprinters = server['Printers']
    if tmpprinters is not None:
        newhtml = newhtml + writeprintershtml(tmpprinters)

    # User Accounts
    tmpuaaccounts = server['UserAccounts']

    newhtml = newhtml + writeuseraccountshtml(tmpuaaccounts)

    # Groups
    tmpgroups = server['Groups']

    newhtml = newhtml + writegroupshtml(tmpgroups)

    # Service Accounts
    tmpservices = server['Services']

    newhtml = newhtml + writeserviceaccountshtml(tmpservices)

    # Services
    tmpservices = server['Services']

    newhtml = newhtml + writeserviceshtml(tmpservices)

    # Appendix
    newhtml = newhtml + writeappendixhtml()

    # write ending html
    newhtml = newhtml + writeendinghtml()

    # write newhtml string and close file
    f2.write(newhtml)
    f2.close()

