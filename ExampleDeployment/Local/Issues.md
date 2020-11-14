# Windows - WSL
Currently with the Docker for WSL the cert permissions will only be set as root. As a workaround for now set nginx to also run as root for Windows development.

Root must be specified to override the dockerfile user.
```
    "User": 0,
    "Group": 0,
```

Don't do this for anything production. Somehow the permission need to be changed in wsl to user 9999 then it should work.