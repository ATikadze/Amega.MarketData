# Amega: Market Data

#### Requirements
* Visual Studio Code
* Browser of your choice (I am using Edge)
* .Net 8.0 installed

#### Usage guide
Everything for comfortable testing is set up. launch.json and tasks.json files are configured.
The application is configured to start on port 7194. So that's what the front end is looking at.
Steps to test out the program:
1. Start the API (make sure to accept/create the http certificate)
2. Open Index.html (I tested it in Edge)
3. The options are going to be listed from the result of an endpoint response when the page loads
4. Click Get Price or Subscribe buttons after selecting the symbol
5. Get Price will trigger an endpoint and retrieve the price for the corresponding symbol
6. Subscribe button will connect to the web socket and occasionally retrieve updates of the price of the symbol

#### What can be improved
This program is an MVP. So there are aspects that I would improve in a real life application, both performance-wise and structurally for a cleaner code. Here are the key highlights however I left these comments and more inside the project itself.
* Unit tests can be added and Moq can be used for more in-depth testing
* Place certain data into app configs
* When an error occurs the error message can be sent to the corresponding socket and then displayed on the page
* Handle web socket errors properly
* A major improvement in performance would be to send subscriptions as batches. So instead of subscribing to the Binance stream individually, as it's being done right now, we can subscribe multiple symbols to the stream. For example, right now I have three different subscriptions for three different symbols. Which means that I am going to receive three times as many messages. Instead, we should subscribe multiple symbols to the ticker stream. And instead of receiving messages for each symbol, we are going to receive all the information about the symbols in one message. This will significantly improve the performance and instead of having a job running per symbol, we're going to have one job in total, which will be receiving information for all symbols
* Fire and forget is a bad approach. Instead, ReceiveMessagesAsync should be run as a job, using a separate Hosted Background Service or Hangfire
* Script for checking closed connections and removing them can be improved significantly
* Binance may close the connection, either suddenly or after 24 hours. We need to detect that and reopen the connection. In case of a failure, we can implement a retry functionality to occasionally ping the Binance server and once it's up then connect to it. Similar to Circuit Breaker pattern.
