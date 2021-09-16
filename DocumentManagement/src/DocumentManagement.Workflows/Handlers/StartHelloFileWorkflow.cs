using System.Threading;
using System.Threading.Tasks;
using DocumentManagement.Core.Events;
using Elsa.Models;
using Elsa.Services;
using MediatR;

namespace DocumentManagement.Workflows.Handlers
{
    /// <summary>
    /// Handles the <see cref="NewDocumentReceived"/> event by starting the HelloFile workflow.
    /// </summary>
    public class StartHelloFileWorkflow : INotificationHandler<NewDocumentReceived>
    {
        private readonly IWorkflowRegistry _workflowRegistry;
        private readonly IWorkflowDefinitionDispatcher _workflowDispatcher;

        public StartHelloFileWorkflow(IWorkflowRegistry workflowRegistry, IWorkflowDefinitionDispatcher workflowDispatcher)
        {
            _workflowRegistry = workflowRegistry;
            _workflowDispatcher = workflowDispatcher;
        }

        public async Task Handle(NewDocumentReceived notification, CancellationToken cancellationToken)
        {
            var document = notification.Document;

            // Get our HelloFile workflow.
            var workflow = await _workflowRegistry.FindAsync(x => x.Name == "HelloFile" && x.IsPublished, cancellationToken);

            if (workflow == null)
                return; // Do nothing.

            // Dispatch the workflow.
            await _workflowDispatcher.DispatchAsync(new ExecuteWorkflowDefinitionRequest(workflow!.Id, Input: new WorkflowInput(document.Id)), cancellationToken);
        }
    }
}