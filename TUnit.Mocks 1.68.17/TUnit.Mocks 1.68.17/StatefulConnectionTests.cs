namespace TUnit.Mocks_1._68._17;

public interface IConnection
{
    event EventHandler<string>? MessageReceived;

    string Status { get; }

    void Connect();

    void Disconnect();

    void Send(string payload);
}

public class StatefulConnectionTests
{
    [Test]
    public async Task Status_follows_the_connection_state_machine()
    {
        // Setups can be scoped to named states, with calls driving the transitions —
        // a protocol mock without hand-rolled booleans.
        var mock = IConnection.Mock();
        mock.SetState("offline");
        mock.InState("offline", m =>
        {
            m.Status.Returns("OFFLINE");
            m.Connect().TransitionsTo("online");
        });
        mock.InState("online", m =>
        {
            m.Status.Returns("ONLINE");
            m.Disconnect().TransitionsTo("offline");
        });

        IConnection connection = mock;

        await Assert.That(connection.Status).IsEqualTo("OFFLINE");

        connection.Connect();
        await Assert.That(connection.Status).IsEqualTo("ONLINE");

        connection.Disconnect();
        await Assert.That(connection.Status).IsEqualTo("OFFLINE");
    }

    [Test]
    public async Task Send_can_auto_raise_the_reply_event()
    {
        var mock = IConnection.Mock();
        var received = new List<string>();

        IConnection connection = mock;
        connection.MessageReceived += (_, message) => received.Add(message);

        // .Raises{EventName}() fires the event whenever the setup is hit …
        mock.Send(Any()).RaisesMessageReceived("ack");
        connection.Send("ping");

        // … and Raise{EventName}() fires it directly.
        mock.RaiseMessageReceived("broadcast");

        await Assert.That(received).Count().IsEqualTo(2);
        await Assert.That(received[0]).IsEqualTo("ack");
        await Assert.That(received[1]).IsEqualTo("broadcast");
    }

    [Test]
    public async Task Subscriptions_are_observable()
    {
        var mock = IConnection.Mock();

        IConnection connection = mock;
        connection.MessageReceived += (_, _) => { };

        await Assert.That(mock.Events.MessageReceived.WasSubscribed).IsTrue();
        await Assert.That(mock.Events.MessageReceived.SubscriberCount).IsEqualTo(1);
    }
}
