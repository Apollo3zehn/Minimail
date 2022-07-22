# Minimail

[![GitHub Actions](https://github.com/Apollo3zehn/Minimail/actions/workflows/build-and-publish.yml/badge.svg)](https://github.com/Apollo3zehn/Minimail/actions)

The purpose of this project is to reduce spam mails by self-hosting a mail server and creating a new mail address for each web service (receive-only, no outgoing mails!). As soon as the address receives spam, it can be removed from the list of addresses using the web interface (port 8080).

Minimail is a very simple mail server based on the [SmtpServer](https://github.com/cosullivan/SmtpServer) library with Maildir support (e.g. for [Dovecot imap server](https://www.dovecot.org/)).

The current version assumes that the web server is running under the subdirectory /minimail, e.g. `http://mydomain:8080/minimail`.

## Docker-Compose Sample Configuration

```yaml
version: "3.7"

services:

  minimail:
    container_name: minimail
    image: apollo3zehn/minimail:1.0.0.alpha.1.12
    environment:
      - MINIMAIL_GENERAL__DOMAIN=<your domain>
      - MINIMAIL_PATHS__CERTFULLCHAIN=<your cert path>/fullchain.pem
      - MINIMAIL_PATHS__CERTPRIVATEKEY=<your key path>/privkey.pem
    ports:
      - "127.0.0.1:8080:8080"   # for GUI
      - "0.0.0.0:25:25"         # for receiving mails
    volumes:
      # this is where the Maildir folder is created
      - <your volume path>/minimail:/var/lib/minimail
    sysctls:
      - net.ipv4.ip_unprivileged_port_start=25
    user: "1000:1000"
    restart: always
```