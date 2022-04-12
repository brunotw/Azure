using System.Threading.Tasks;

namespace DurableEntitiesCounter
{
    //Restrictions on entity interfaces
    //As usual, all parameter and return types must be JSON-serializable.Otherwise, serialization exceptions are thrown at runtime.

    //We also enforce some additional rules:
    // * Entity interfaces must only define methods.
    // * Entity interfaces must not contain generic parameters.
    // * Entity interface methods must not have more than one parameter.
    // * Entity interface methods must return void, Task, or Task<T>
    public interface ICounter
    {
        Task<int> Get();
    }
}
