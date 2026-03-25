public sealed class AppDbContext
{
	public Supabase.Client Client { get; }

	public AppDbContext(Supabase.Client client)
	{
		Client = client;
	}
}
