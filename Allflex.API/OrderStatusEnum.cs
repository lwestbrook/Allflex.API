namespace Allflex.API
{
    public enum OrderStatusEnum
    {
        Invalid = 0,
        Created,
        Error,
        Hold,
        CustomerUpdated,
        Confirmed,
        InSelection, // Job In Review / Prestage
        Selected, // Job Scheduled / Queued
        InProduction,// In Production (%)
        QcChecked,// QC Checked (%)
        Dispatched, //
        Shipped,
        Invoiced,
        Canceled
    }
}
