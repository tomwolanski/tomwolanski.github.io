<Query Kind="Program">
  <Connection>
    <ID>54bf9502-9daf-4093-88e8-7177c12aaaaa</ID>
    <NamingService>2</NamingService>
    <Driver Assembly="(internal)" PublicKeyToken="no-strong-name">LINQPad.Drivers.EFCore.DynamicDriver</Driver>
    <AttachFileName>&lt;ApplicationData&gt;\LINQPad\ChinookDemoDb.sqlite</AttachFileName>
    <DisplayName>Demo database (SQLite)</DisplayName>
    <DriverData>
      <PreserveNumeric1>true</PreserveNumeric1>
      <EFProvider>Microsoft.EntityFrameworkCore.Sqlite</EFProvider>
      <MapSQLiteDateTimes>true</MapSQLiteDateTimes>
      <MapSQLiteBooleans>true</MapSQLiteBooleans>
    </DriverData>
  </Connection>
  <DisableMyExtensions>true</DisableMyExtensions>
</Query>


DbSet<FullInvoiceDetails> InvoiceDetails => this.Set<FullInvoiceDetails>();

void Main()
{
	EnsureView();
	
	InvoiceDetails
		//.AsNoTracking()
		.Where(n => n.InvoiceId == 1)
		.ToArray()
		.Dump();

}



protected override void OnModelCreating(ModelBuilder modelBuilder)
{
	base.OnModelCreating(modelBuilder);

	modelBuilder.Entity<FullInvoiceDetails>(b => 
	{
	    b.ToView("FullInvoiceDetails");
		
		b.HasKey(n => n.InvoiceId);
		//b.HasKey(n => new { n.InvoiceId, n.InvoiceLineId });
		//b.HasNoKey();
	});
}

void EnsureView()
{
	var sql = """
	create view if not exists FullInvoiceDetails 
	(InvoiceId, InvoiceDate, BillingAddress, BillingCountry, Total, InvoiceLineId, UnitPrice, Quantity, TrackId) as
	select i.InvoiceId, i.InvoiceDate, i.BillingAddress, i.BillingCountry, i.Total, l.InvoiceLineId, l.UnitPrice, l.Quantity, l.TrackId
	from Invoice i
	join InvoiceLine l on i.InvoiceId = l.InvoiceId
	""";
	
	this.Database.ExecuteSqlRaw(sql);
}




public class FullInvoiceDetails
{
	public int InvoiceId { get; init; }
	public int TrackId {get; init; }
	public int Quantity {get; init;}
	public int InvoiceLineId {get; init;}
	public decimal Total {get; init;}
	public decimal UnitPrice {get; init;}
	public string BillingCountry {get; init;}
	public string BillingAddress {get; init;}
	public string InvoiceDate {get; init;}	
}
