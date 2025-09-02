# Integrating TagzApp with Your Application using Azure Storage Queues

The `MessageClient` class, provided by the `TagzApp.Lib.AzureQueue` package, enables seamless integration of your application or website with TagzApp by leveraging Azure Storage Queues. This allows you to send messages that will appear on TagzApp in real-time. Ensure that the `TagzApp.Lib.AzureQueue` package is installed in your project before proceeding.

### Steps to Configure and Use the MessageClient

1. **Set Up Azure Queue**:
   - Create an Azure Storage account if you don't already have one.
   - Create a queue in the Azure Storage account.
   - Note down the connection string and queue name.

2. **Integrate MessageClient in Your Application**:
   - Add a reference to the `TagzApp.Lib.AzureQueue` library in your project.
   - Use the `MessageClient` class to send messages to the Azure Queue.

3. **Code Example**:
   ```csharp
   using TagzApp.Lib.AzureQueue;

   // Initialize the MessageClient with your Azure Storage connection string and queue name
   var messageClient = new MessageClient("<YourConnectionString>", "<YourQueueName>");

   // Submit a message to the queue
   await messageClient.SubmitMessage("Hello, TagzApp!", "AuthorName");
   ```

4. **Run Your Application**:
   - Ensure your application has network access to the Azure Storage account.
   - Messages sent using the `MessageClient` will appear on TagzApp with a globe icon for the provider.
