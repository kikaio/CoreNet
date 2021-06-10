using MySql.Data.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreNet.DB
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class CoreDbContext : DbContext
    {
        protected CoreDbContext(string _connStr)
            : base(_connStr)
        {

        }

        public virtual void DoActAndSave(Action _act)
        {
            using (var conn = this.Database.Connection)
            {
                conn.Open();
                DoWithTransaction(() => {
                    _act();
                    SaveChanges();
                });
            }
        }

        public virtual async Task DoActAndSaveAsync(Action _act)
        {
            using (var conn = this.Database.Connection)
            {
                await conn.OpenAsync();
                if (_act != null)
                {
                    await DoAsyncWithTransaction(async () =>
                    {
                        await Task.Factory.StartNew(_act);
                        await SaveChangesAsync();
                    });
                }
            }
        }

        //단지 transaction을 감싸줄뿐 context에 대한 save등은 직접 호출해야함.
        private void DoWithTransaction(Action _act)
        {
            try
            {
                using (var tr = this.Database.BeginTransaction())
                {
                    _act?.Invoke();
                    tr.Commit();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private async Task DoAsyncWithTransaction(Action _act)
        {
            try
            {
                using (var tr = Database.BeginTransaction())
                {
                    await Task.Factory.StartNew(_act);
                    tr.Commit();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
