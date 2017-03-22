-- ****************************** Module Header ******************************
-- Module Name:  CorpMailman Windows Service project.
-- Project:      PostDaemon: Windows Service to send out emails project.
--
-- MailerDB SQL file to create the database, load email tables for CorpMailman to work - please employ MySQL 5.5 or greater
--
-- 1.    T1:    mails table - all emails that we need to send added here
-- 2.    T2.    mailAttachments table - individual attachments that we send with emails
-- 3.    T3.    mailsSent table - log for emails that we send out
-- 5.    P1.    addEmail stored procedure - add an email to mails table, with direct and timestamp values
-- 6.    P2.    addMailAttachment stored procedure - add attachment to an existing email
-- 7.    P3.    markMailAsReady stored procedure - mark an email as ready, so that you can dispatch it (after adding attachments)
-- 8.    P4.    getEmailToSend stored procedure - get the next email to send
-- 9.    P5.    getAttachmentsForEmail stored procedure - get attachments for a certain email
-- 10.   P6.    deleteEmail stored procedure - log the email being sent and delete that email
-- 11.   P7.    addConstraint stored procedure - add constraint from attachments to emails
-- 12.   P8.    addEmailNew stored procedure - shortcut method to add new Email without cc, bcc, importance
--
-- Revisions:
--      1. Sundar Krishnamurthy         sundar_k@hotmail.com               8/27/2015       Initial file created.
-- ***************************************************************************/

-- Very, very, very bad things happen if you uncomment this line below. Do at your peril, you have been warned!
-- drop database if exists $$MAIL_DATABASE$$;                                                     -- $$ MAIL_DATABASE $$

-- Create database $$MAIL_DATABASE$$, with utf8 and utf8_general_ci
create database if not exists $$MAIL_DATABASE$$ character set utf8 collate utf8_general_ci;       -- $$ MAIL_DATABASE $$

-- Employ $$MAIL_DATABASE$$
use $$MAIL_DATABASE$$;                                                                            -- $$ MAIL_DATABASE $$

-- drop table if exists mails;

-- 1. T1. mails table to store emails that need to be dispatched
create table if not exists mails (
    mailId                                    int ( 10 ) unsigned              not null auto_increment,
    sender                                    varchar( 256 )                   default null,
    recipients                                varchar( 4096 )                  not null,
    ccRecipients                              varchar( 4096 )                  default null,
    bccRecipients                             varchar( 4096 )                  default null,
    subject                                   varchar( 255 )                   not null,
    subjectPrefix                             varchar( 64 )                    default null,
    body                                      text                             default null,
    ready                                     tinyint ( 1 ) unsigned           not null default 0,
    hasAttachments                            tinyint ( 1 ) unsigned           not null default 0,
    importance                                tinyint ( 1 ) unsigned           not null default 0,
    direct                                    tinyint ( 1 ) unsigned           not null default 0,
    timestamp                                 datetime                         default null,
    created                                   datetime                         not null,
    key ( mailId )
) ENGINE=InnoDB DEFAULT CHARACTER SET=utf8;

-- drop table if exists mailAttachments;

-- 2. T2. mailAttachments for what needs to be sent as attachments
create table if not exists mailAttachments (
    mailAttachmentId                         int ( 10 ) unsigned              not null auto_increment,
    mailId                                   int ( 10 ) unsigned              not null,
    filename                                 varchar( 1024 )                  not null,
    filesize                                 int ( 10 ) unsigned              not null,
    attachment                               longblob                         not null,
    created                                  datetime                         not null,
    key ( mailAttachmentId )
) ENGINE=InnoDB DEFAULT CHARACTER SET=utf8;

-- drop table if exists mailsSent;

-- 3. T3. mailSent table to log all the emails we successfully dispatch
create table if not exists mailsSent (
    logId                                     int ( 10 ) unsigned              not null auto_increment,
    recipients                                varchar( 16384 )                 not null,
    subject                                   varchar( 255 )                   not null,
    timestamp                                 datetime default null,
    key ( logId )
) ENGINE=InnoDB DEFAULT CHARACTER SET=utf8;

drop procedure if exists addEmail;

delimiter //

-- 4. P1 - add new email to mails table, mail would not be dispatched unless all attachments are in too
create procedure addEmail (
    in p_sender                              varchar( 256 ),
    in p_recipients                          varchar( 4096 ),
    in p_ccRecipients                        varchar( 4096 ),
    in p_bccRecipients                       varchar( 4096 ),
    in p_subject                             varchar( 255 ),
    in p_subjectPrefix                       varchar( 64 ),
    in p_body                                text,
    in p_markMailAsReady                     tinyint ( 1 ) unsigned,
    in p_hasAttachments                      tinyint ( 1 ) unsigned,
    in p_importance                          tinyint ( 1 ) unsigned,
    in p_direct                              tinyint ( 1 ) unsigned,
    in p_timestamp                           datetime
)
begin

    insert mails (
        sender,
        recipients,
        ccRecipients,
        bccRecipients,
        subject,
        subjectPrefix,
        body,
        ready,
        hasAttachments,
        importance,
        direct,
        timestamp,
        created
    ) values (
        p_sender,
        p_recipients,
        p_ccRecipients,
        p_bccRecipients,
        p_subject,
        p_subjectPrefix,
        p_body,
        p_markMailAsReady,
        p_hasAttachments,
        p_importance,
        p_direct,
        p_timestamp,
        utc_timestamp()
    );

    select last_insert_id() as mailId;
end //

delimiter ;

drop procedure if exists addMailAttachment;

delimiter //

-- 6. P2 - Add one attachment to a previously saved email
create procedure addMailAttachment (
    in p_mailId                              int ( 10 ) unsigned,
    in p_filename                            varchar ( 1024 ),
    in p_filesize                            int ( 10 ) unsigned,
    in p_attachment                          longblob
)
begin

    declare l_mailId             int ( 10 ) unsigned;
    declare l_hasAttachments     bit;

    set l_mailId = null;
    set l_hasAttachments = null;

    select
        hasAttachments, mailId
    into
        l_hasAttachments, l_mailId
    from
        mails
    where
        mailId = p_mailId;

    if l_hasAttachments is not null and l_hasAttachments = 0 then
        update
            mails
        set
            hasAttachments = 1
        where
            mailId = p_mailId;
    end if;

    if l_mailId is not null then
        insert mailAttachments (
            mailId,
            filename,
            filesize,
            attachment,
            created
        ) values (
            p_mailId,
            p_filename,
            p_filesize,
            p_attachment,
            utc_timestamp()
        );

        select last_insert_id() as mailAttachmentId;
    else
        select null as mailAttachmentId;
    end if;
end //

delimiter ;

drop procedure if exists markEmailAsReady;

delimiter //

-- 7. P3 - Mark email as ready to send
create procedure markEmailAsReady (
    in p_mailId                              int ( 10 ) unsigned
)
begin

    declare l_ready bit;
    set l_ready = null;

    select
        ready into l_ready
    from
        mails
    where
        mailId = p_mailId;

    if l_ready is not null and l_ready = 0 then
        update
            mails
        set
            ready = 1
        where
            mailId = p_mailId;

    end if;

    select p_mailId as mailId;
end //

delimiter ;

drop procedure if exists getEmailToSend;

delimiter //

-- 8. P4 - Get the next email to dispatch, hasAttachments would tell you if you need to call getAttachmentsForEmail
create procedure getEmailToSend (
)
begin

    select
        mailId,
        sender,
        recipients,
        subject,
        subjectPrefix,
        ccRecipients,
        bccRecipients,
        body,
        hasAttachments,
        importance,
        created,
        direct
    from
        mails
    where
            ready = 1
        and
            ((timestamp is null) or (timestamp < utc_timestamp()))
    order by
        mailId
    limit 1;
end //

delimiter ;

drop procedure if exists getAttachmentsForEmail;

delimiter //

-- 9. P5 - Get Attachments that are defined or were added for this email, assuming ready is still set to false for this mailId
create procedure getAttachmentsForEmail (
    in p_mailId                              int ( 10 ) unsigned
)
begin

    select
        mailAttachmentId,
        mailId,
        filename,
        filesize,
        attachment,
        created
    from
        mailAttachments
    where
        mailId = p_mailId
    order by
        mailAttachmentId;
end //

delimiter ;

drop procedure if exists deleteEmail;

delimiter //

-- 10. P6 - Delete this email, we have successfully dispatched it into the ether
create procedure deleteEmail (
    in p_mailId                              int ( 10 ) unsigned
)
begin

    -- Log this email that we dispatched
    insert mailsSent (
        recipients,
        subject,
        timestamp
    ) select
       recipients,
       subject,
       utc_timestamp()
    from
       mails
    where
       mailId = p_mailId;

    -- This is moot, but still delete attachments prior to deleting emails
    delete
    from
        mailAttachments
    where
        mailId = p_mailId;

    delete
    from
        mails
    where
        mailId = p_mailId;

    select p_mailId as mailId;
end //

-- 11. P7 - Add the constraint if it does not exist
create procedure addConstraint()
begin
    -- 2a. Add constraint for mailAttachments to mails
    if not exists (select * from information_schema.TABLE_CONSTRAINTS where
                   CONSTRAINT_SCHEMA = DATABASE() and
                   CONSTRAINT_NAME   = 'fk_mailAttachments_mails_mailId' and
                   CONSTRAINT_TYPE   = 'FOREIGN KEY') then

        alter table
            mailAttachments
        add constraint
            fk_mailAttachments_mails_mailId
        foreign key (mailId)
        references mails (mailId)
        on update cascade
        on delete cascade;
    end if;
end //

delimiter ;

call addConstraint();

-- Drop this anyways, we don't need 7 forward (addConstraint)
drop procedure if exists addConstraint;

drop procedure if exists addEmailNew;

delimiter //

-- 12. P8 - Shortcut method to add new emails without cc, bcc, importance etc.
create procedure addEmailNew (
    in p_recipients                          varchar( 4096 ),
    in p_subject                             varchar( 255 ),
    in p_body                                text,
    in p_hasAttachments                      tinyint ( 1 ) unsigned,
    in p_markMailAsReady                     tinyint ( 1 ) unsigned,
    in p_direct                              tinyint ( 1 ) unsigned,
    in p_timestamp                           datetime
)
begin

    declare l_direct                         tinyint ( 1 ) unsigned;
    set l_direct = 0;

    if p_direct is not null then
        set l_direct = p_direct;
    end if;

    insert mails (
        recipients,
        subject,
        body,
        hasAttachments,
        ready,
        created,
        direct,
        timestamp
    ) values (
        p_recipients,
        p_subject,
        p_body,
        p_hasAttachments,
        p_markMailAsReady,
        utc_timestamp(),
        l_direct,
        p_timestamp
    );

    select last_insert_id() as mailId;
end //

delimiter ;
