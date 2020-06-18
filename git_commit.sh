#!/bin/bash
eval `ssh-agent`
ssh-add ~/.ssh/win10.priv.ppk
git pull
git add -A
git commit -m "autocommit"
git push
