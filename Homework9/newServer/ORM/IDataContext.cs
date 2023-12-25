namespace newServer.ORM;

public interface IDataContext 
{
    bool Add<T>(T entity);
    bool Update<T>(T entity);
    bool Delete<T>(int id);
    List<T> Select<T>(T entity); 
    T SelectById<T>(string id);
}