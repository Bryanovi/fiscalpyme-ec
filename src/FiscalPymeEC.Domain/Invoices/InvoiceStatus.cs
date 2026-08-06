using System;
using System.Collections.Generic;
using System.Text;

namespace FiscalPymeEC.Domain.Invoices;

public enum InvoiceStatus
{
    Draft = 1,
    Issued = 2,
    XmlGenerated = 3,
    Signed = 4,
    Received = 5,
    Authorized = 6,
    Rejected = 7
}
