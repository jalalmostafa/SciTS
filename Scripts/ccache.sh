#! /bin/bash
if [ $# -gt 0 ]; then
    systemctl stop $1
    sync && echo 3 > /proc/sys/vm/drop_caches
    systemctl start $1
else
    echo "Usage: ccache.sh <service_name>"
    echo "service_name is missing"
fi
