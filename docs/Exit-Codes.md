# `dotnet redact-binlog` exit codes and their meaning

Exit codes are chosen to conform to existing standards or standardization attempts and well known exit code. See [Related resources](#related) for more details 

| Exit&nbsp;Code | Reason |
|:-----|----------|
| 0 | Success |
| [65](#65) | Invalid, corrupted or unexpected data. |
| [70](#70) | Unexpected internal software issue. |
| [73](#73) | Can't create output file. |
| [102](#102) | Missing required option(s) and/or argument(s) for the command. |
| [107 - 113](#107) | Reserved. |
| [127](#127) | Unrecognized option(s) and/or argument(s) for a command. |
| [130](#130) | Command terminated by user. |


To enable verbose logging in order to troubleshoot issue(s), use the `-v` (console) or set the `DOTNET_CLI_CONTEXT_VERBOSE` environment variable to `true`

_PowerShell:_
```PowerShell
$env:DOTNET_CLI_CONTEXT_VERBOSE = 'true'
```

_Cmd:_
```cmd
set DOTNET_CLI_CONTEXT_VERBOSE=true
```

## <a name="65"></a>65 - Invalid, corrupted or unexpected data

Invalid, corrupted, unexpected data - mostly means corrupted input log.

This is a semi-standardized exit code (see [EX_DATAERR in /usr/include/sysexits.h](https://github.com/openbsd/src/blob/master/include/sysexits.h#L102))


## <a name="70"></a>70 - Unexpected internal software issue

Unexpected result or issue. [File a bug](https://github.com/JanKrivanek/MSBuildBinlogRedactor/issues/new?title=Unexpected%20Internal%20Software%20Issue%20(EX_SOFTWARE)) if you encounter this exit code.

This is a semi-standardized exit code (see [EX_SOFTWARE in /usr/include/sysexits.h](https://github.com/openbsd/src/blob/master/include/sysexits.h#L107))


## <a name="73"></a>73 - Can't create output file.

Destination file already exists and force write is not requested - so command cannot proceed without destructive changes..

Destructive changes can be forced by passing `--force` option.

This is a semi-standardized exit code (see [EX_CANTCREAT in /usr/include/sysexits.h](https://github.com/openbsd/src/blob/master/include/sysexits.h#L110))


## <a name="102"></a>102 - Missing required option(s) and/or argument(s) for the command

_Reserved for future usage - described behavior is only partially implemented. Some cases that should fall under this exit code are now leading to code [127](#127).


## <a name="107"></a><a name="108"></a><a name="109"></a><a name="110"></a><a name="111"></a><a name="112"></a><a name="113"></a>107 - 113

Reserved for future use.


## <a name="127"></a>127 - Unrecognized option(s) and/or argument(s) for a command

The exit code is used when one or more options or/and arguments used in the command not recognized or invalid. 

This is a semi-standardized exit code (see [127 - "command not found" in 'The Linux Documentation Project'](https://tldp.org/LDP/abs/html/exitcodes.html))


## <a name="130"></a>130 - Command terminated by user.

The exit code is used if command is terminated after user non-forceful termination request (e.g. `Ctrl-C`, `Ctrl-Break`).

This is a semi-standardized exit code (see [130 - Script terminated by Control-C in 'The Linux Documentation Project'](https://tldp.org/LDP/abs/html/exitcodes.html))

<BR/>
<BR/>
<BR/>

### Related Resources
* [`BSD sysexit.h`](https://github.com/openbsd/src/blob/master/include/sysexits.h)
* [`Special exit codes - The Linux Documentation Project`](https://tldp.org/LDP/abs/html/exitcodes.html)
