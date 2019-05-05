#!/bin/bash
echo "Adding development user"
useradd devuser -u 3000 -s /bin/bash -p "$(openssl passwd -1 devuser)" -m -g sudo
