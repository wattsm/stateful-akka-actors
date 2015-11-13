using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka;
using Akka.Actor;

namespace CSharp {

    public static class Messages {

        public class StartCollecting {

            public StartCollecting(int count) {
                this.Count = count;
            }

            public int Count { get; set; }
        }

        public class StopCollecting {
        }

        public class Collected {

            public Collected(string value) {
                this.Value = value;
            }

            public string Value { get; set; }
        }

    }

    public class ExampleActor : ReceiveActor {

        private int _count = 0;
        private readonly List<String> _values = new List<string>();

        public ExampleActor() {
            this.Waiting();
        }

        private void Waiting() {
            this.Receive<Messages.StartCollecting>(message => {

                Console.WriteLine("Starting collection");

                _values.Clear();
                _count = message.Count;

                this.Become(this.Collecting);
            });
        }

        private void Collecting() {
            this.Receive<Messages.Collected>(message => {
                
                _values.Add(message.Value);

                if(_values.Count() == _count) {

                    Console.WriteLine(
                        "Finished collecting ({0}/{0}): {1}", 
                        _count, 
                        String.Join(",", _values)
                    );

                    this.StopCollecting();
                }
            });

            this.Receive<Messages.StopCollecting>(message => {

                Console.WriteLine(
                    "Stopped collecting ({0}/{1}): {2}", 
                    _values.Count(), 
                    _count, 
                    String.Join(",", _values)
                );

                this.StopCollecting();
            });
        }

        private void StopCollecting() {
            this.Become(this.Waiting);

            //Or stop
            //ReceiveActor.Context.Stop(this.Self);
        }
    }

    class Program {
        static void Main(string[] args) {

            var system = ActorSystem.Create("ExampleSystem");
            var actor = system.ActorOf<ExampleActor>("ExampleActor");

            actor.Tell(new Messages.Collected("Hello, World (0)")); //Unhandled
            actor.Tell(new Messages.StartCollecting(3));
            actor.Tell(new Messages.Collected("Hello, World (1)"));
            actor.Tell(new Messages.Collected("Hello, World (2)"));
            actor.Tell(new Messages.Collected("Hello, World (3)"));
            actor.Tell(new Messages.Collected("Hello, World (4)")); //Unhandled (or dead letter if actor stopped)

            Console.ReadLine();

            system.Shutdown();
        }
    }
}
