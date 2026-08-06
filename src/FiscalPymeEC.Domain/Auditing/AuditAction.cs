using System;
using System.Collections.Generic;
using System.Text;

namespace FiscalPymeEC.Domain.Auditing;

public enum AuditAction
{
    Created = 1,
    Updated = 2,
    Activated = 3,
    Deactivated = 4,
    LoginSucceeded = 5,
    LoginFailed = 6,
    InvoiceIssued = 7,
    XmlGenerated = 8,
    InvoiceSigned = 9,
    SentToSri = 10,
    InvoiceAuthorized = 11,
    InvoiceRejected = 12,
    Deleted = 13
}
