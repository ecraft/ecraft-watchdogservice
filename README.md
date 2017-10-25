# ecraft-watchdogservice

A generic watchdog Windows Service for keeping processes running 24/7.

If a process dies for whatever reason (out of memory, segmentation fault etc), the watchdog service will detect this and automatically restart the process. An arbitrary number of processes can be controlled using a single watchdog service.

Each process will have its own log file, which is rotated daily (the year, month and date is included in the log file name). To avoid running out of disk space, old log files are deleted every time the service starts. (Note: if the service is never restarted, this means that old logs will be kept indefinitely.)

## Configuration

The service is configured with a file called `run_config.xml` which is expected to be located in the same folder as the `watchdogservice.exe` file. A somewhat outdated file can be found in [eCraft.appFactory.appFactoryService/run_config.xml](eCraft.appFactory.appFactoryService/run_config.xml)

## Installation/uninstallation

The service has built-in support for installing itself as a Windows Service. Use the commands below from an Administrator/elevated command prompt:

```cmd
watchdogservice /i [env]
wwatchdogservice /u [env]
```

The `env` is optional; ifused, it will be appended to the service name installed. This is useful if you are running multiple environments on the same machine.

## Starting the service

```cmd
net start ecraft_watchdog
net stop ecraft_watchdog
```

If you used the `env` option above, the name is instead `ecraft_watchdog_<env>`.

## License

MIT
