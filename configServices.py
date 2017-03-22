import os;

dbServerName = "";
dbUsername = "";
dbPassword = "";
dbName = "";
mailCredential = "";
smtpServer = "";
sendAsName = "";
samAccountName = "";
samAccountPassword = "";
yourEmailAddress = "";
yourCcEmailAddress = "";
yourBccEmailAddress = "";

useEws = "";
ewsUrl = "";

with open("CorpMailman.exe.config.out", "wt") as fout:
    with open("CorpMailman.exe.config", "rt") as fin:
        for line in fin:

            nextLine = line;

            if ("$$MYSQL_ENDPOINT$$" in line)              : nextLine = line.replace("$$MYSQL_ENDPOINT$$", dbServerName);
            if ("$$MYSQL_LOGIN$$" in line)                 : nextLine = nextLine.replace("$$MYSQL_LOGIN$$", dbUsername);
            if ("$$MYSQL_PASSWORD$$" in line)              : nextLine = nextLine.replace("$$MYSQL_PASSWORD$$", dbPassword);
            if ("$$MAIL_DATABASE$$" in line)               : nextLine = nextLine.replace("$$MAIL_DATABASE$$", dbName);
            if ("$$MAIL_CREDENTIALS$$" in line)            : nextLine = nextLine.replace("$$MAIL_CREDENTIALS$$", mailCredential);
            if ("$$SMTP_SERVER$$" in line)                 : nextLine = nextLine.replace("$$SMTP_SERVER$$", smtpServer);
            if ("$$SENDAS_NAME$$" in line)                 : nextLine = nextLine.replace("$$SENDAS_NAME$$", sendAsName);
            if ("$$SAM_ACCOUNT_NAME$$" in line)            : nextLine = nextLine.replace("$$SAM_ACCOUNT_NAME$$", samAccountName);
            if ("$$SAM_ACCOUNT_PASSWORD$$" in line)        : nextLine = nextLine.replace("$$SAM_ACCOUNT_PASSWORD$$", samAccountPassword);
            if ("$$YOUR_EMAIL_ADDRESS$$" in line)          : nextLine = nextLine.replace("$$YOUR_EMAIL_ADDRESS$$", yourEmailAddress);
            if ("$$YOUR_CC_EMAIL_ADDRESS$$" in line)       : nextLine = nextLine.replace("$$YOUR_CC_EMAIL_ADDRESS$$", yourCcEmailAddress);
            if ("$$YOUR_BCC_EMAIL_ADDRESS$$" in line)      : nextLine = nextLine.replace("$$YOUR_BCC_EMAIL_ADDRESS$$", yourBccEmailAddress);
            if ("$$USE_EWS$$" in line)                     : nextLine = nextLine.replace("$$USE_EWS$$", useEws);
            if ("$$EWS_URL$$" in line)                     : nextLine = nextLine.replace("$$EWS_URL$$", ewsUrl);

            fout.write(nextLine);

os.remove("CorpMailman.exe.config");
os.rename("CorpMailman.exe.config.out", "CorpMailman.exe.config");

with open("mailerdb.sql.out", "wt") as fout:
    with open("mailerdb.sql", "rt") as fin:
        for line in fin:

            nextLine = line;

            if ("$$MAIL_DATABASE$$" in line)               : nextLine = line.replace("$$MAIL_DATABASE$$", dbName);

            fout.write(nextLine);

os.remove("mailerdb.sql");
os.rename("mailerdb.sql.out", "mailerdb.sql");
print("Complete.");
