
# SystemsDocument

## Requirements:

### Hardware Requirements: (does not include requirements to run the operating system)

Initial disk space: 10MB
- Log files will be produced; ongoing space requirements will depend on size of the environment and the frequency that SystemsDocument is run

Memory: 16-32MB

### Operating System Requirements:
- Windows Server 2003 w/ SP2
- Windows Server 2003 R2 w/ SP2
- Windows Server 2008*
- Windows Server 2008 R2*
- Windows Server 2012*
- Windows Server 2012 R2*

* excluding Server Core versions

### Additional software requirements:
- Microsoft .NET Framework v3.5

### Additional requirements:
- SystemsDocument requires an active Internet connection from the target system to function properly. SystemsDocument communicates via HTTPS (port 443).
- Features:
    - SystemsDocument process secured by SSL and AES encryption
    - Generated documentation is automatically downloaded to the system that SystemsDocument is executed on
    - Documentation can be saved to a specified filesystem location (including UNC paths)

### Command-line Parameters:
- run (Required)
- v (Optional: minimal verbose output)

Usage:
Open a command-line prompt on the target system for SystemsDocument.
Navigate to the directory where SystemsDocument is installed.
On the command-line, execute SystemsDocument –run.
Documentation will be saved in the directory where SystemsDocument is executed unless specified in SystemsDocument.cfg (see below for more information on SystemsDocument.cfg).

### SystemsDocument.cfg
SystemsDocument.cfg specifies a specific download location for use by SystemsDocument. The specified location can be a local drive on the system, network drive (accessed via drive letter or UNC path).

Format:
SaveLocation|[destination path]
Example:

SaveLocation|C:\Documentation
SaveLocation|\\SERVER1\\Documentation

Note: Before running SystemsDocument, verify access to the UNC path from the target system
using the credentials that will be used to execute SystemsDocument.
