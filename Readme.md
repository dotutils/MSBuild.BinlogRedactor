# `dotnet redact-binlog` command

```
Description:
  Provides ability to redact sensitive data from MSBuild binlogs (https://aka.ms/binlog-redactor).

Usage:
  redact-binlog [options]

Options:
  -p, --password <password> (REQUIRED)  Password or other sensitive data to be redacted from binlog. Multiple options
                                        are supported.
  -i, --input <input>                   Input binary log file name. Or a directory to inspect for all existing binlogs.
                                        If not specified current directory is assumed.
  -o, --output <output>                 Output binary log file name. If not specified, replaces the input file in place
                                        - overwrite option needs to be specified in such case.
  -f, --overwrite                       Replace the output file if it already exists. Replace the input file if the
                                        output file is not specified.
  --dryrun                              Performs the operation in-memory and outputs what would be performed.
  -r, --recurse                         Recurse given path (or current dir if none) for all binlogs. Applies only when
                                        single input file is not specified.
  --logsecrets                          Logs what secrets have been detected and replaced. This should be used only for
                                        test/troubleshooting purposes!
  -v, --verbosity <LEVEL>               Sets the verbosity level. Allowed values are q[uiet], m[inimal], n[ormal], and
                                        diag[nostic]. [default: normal]
  -?, -h, --help                        Show help and usage information
  --version                             Show version information
```
