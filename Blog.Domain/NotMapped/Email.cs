﻿namespace Blog.Domain.NotMapped
{
    public class Email
    {
        public virtual int EmailId { get; set; }
        public virtual string Subject { get; set; }
        public virtual string Body { get; set; }
        public virtual string Body2 { get; set; }
        public virtual string Body3 { get; set; }
        public virtual bool IsHtmlBody { get; set; }
        public virtual DateTime CriadoEm { get; set; }
        public virtual DateTime? EnviadoEm { get; set; }
        public virtual List<EmailAddress> To { get; set; }
        public virtual List<EmailAddress> Cc { get; set; }
        public virtual List<EmailAddress> Bcc { get; set; }
    }
}
