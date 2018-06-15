namespace Allflex.API
{
    public enum OrderStatusEnum
    {
        Created,
        Error,
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
