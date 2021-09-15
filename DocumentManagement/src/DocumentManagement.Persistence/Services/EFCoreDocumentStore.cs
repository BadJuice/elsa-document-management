using System.Threading;
using System.Threading.Tasks;
using DocumentManagement.Core.Models;
using DocumentManagement.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace DocumentManagement.Persistence.Services
{
    public class EFCoreDocumentStore : IDocumentStore
    {
        private readonly IDbContextFactory<DocumentDbContext> _dbContextFactory;

        public EFCoreDocumentStore(IDbContextFactory<DocumentDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        // One improvement one might consider is making the SaveAsync method of the EFCoreDocumentStore
        // class thread-safe by doing an upsert operation server-side rather than client side. Right now,
        // if two threads were to try and save a document object with the same ID at the same time,
        // chances are high that you end up with two records in the database.

        /*
         * To do a server-side upsert operation, you might consider using a 3rd party package such as
         * EFCore.BulkExtensions, FlexLabs.Upsert or Entity Framework Extensions.
        */
        public async Task SaveAsync(Document entity, CancellationToken cancellationToken = default)
        {
            await using var dbContext = _dbContextFactory.CreateDbContext();
            var existingDocument = await dbContext.Documents.FirstOrDefaultAsync(x => x.Id == entity.Id, cancellationToken);
            
            if(existingDocument == null)
                await dbContext.Documents.AddAsync(entity, cancellationToken);
            else
                dbContext.Entry(existingDocument).CurrentValues.SetValues(entity);
            
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<Document?> GetAsync(string id, CancellationToken cancellationToken = default)
        {
            await using var dbContext = _dbContextFactory.CreateDbContext();
            return await dbContext.Documents.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }
    }
}