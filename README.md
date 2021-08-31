# Nexus

[![AppVeyor](https://ci.appveyor.com/api/projects/status/github/apollo3zehn/minimail?svg=true&branch=master)](https://ci.appveyor.com/project/Apollo3zehn/Minimail)

The purpose of this project is to reduce spam mails by self-hosting a mail server and creating a new mail address for each web service (receive-only, no outgoing mails!). As soon as the address receives spam, it can be removed from the list of addresses using the web interface (port 8080).

Minimail is a very simple mail server based on the [SmtpServer](https://github.com/cosullivan/SmtpServer) library with Maildir support (e.g. for [Dovecot imap server](https://www.dovecot.org/)).


