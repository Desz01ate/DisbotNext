using DisbotNext.Infrastructures.Sqlite.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DisbotNext.Infrastructures.Sqlite.CustomDbSets
{
    public class MemberDbSet : DbSet<Member>
    {
        private readonly DbSet<Member> _members;
        public MemberDbSet(DbSet<Member> members)
        {
            this._members = members;
        }

        public override IEntityType EntityType => _members.EntityType;

        public override LocalView<Member> Local => _members.Local;

        public override EntityEntry<Member> Add(Member entity)
        {
            return _members.Add(entity);
        }

        public override ValueTask<EntityEntry<Member>> AddAsync(Member entity, CancellationToken cancellationToken = default)
        {
            return _members.AddAsync(entity, cancellationToken);
        }

        public override void AddRange(params Member[] entities)
        {
            _members.AddRange(entities);
        }

        public override void AddRange(IEnumerable<Member> entities)
        {
            _members.AddRange(entities);
        }

        public override Task AddRangeAsync(params Member[] entities)
        {
            return _members.AddRangeAsync(entities);
        }

        public override Task AddRangeAsync(IEnumerable<Member> entities, CancellationToken cancellationToken = default)
        {
            return _members.AddRangeAsync(entities, cancellationToken);
        }

        public override IAsyncEnumerable<Member> AsAsyncEnumerable()
        {
            return _members.AsAsyncEnumerable();
        }

        public override IQueryable<Member> AsQueryable()
        {
            return _members.AsQueryable();
        }

        public override EntityEntry<Member> Attach(Member entity)
        {
            return _members.Attach(entity);
        }

        public override void AttachRange(params Member[] entities)
        {
            _members.AttachRange(entities);
        }

        public override void AttachRange(IEnumerable<Member> entities)
        {
            _members.AttachRange(entities);
        }

        public override bool Equals(object obj)
        {
            return _members.Equals(obj);
        }

        public override Member Find(params object[] keyValues)
        {
            return _members.Find(keyValues);
        }

        public override async ValueTask<Member> FindAsync(params object[] keyValues)
        {
            var id = (ulong)keyValues[0];
            var member = await _members.FindAsync(id);
            if (member == null)
            {
                member = Member.NewMember(id);
                await _members.AddAsync(member);
            }
            return member;
        }

        public override ValueTask<Member> FindAsync(object[] keyValues, CancellationToken cancellationToken)
        {
            return _members.FindAsync(keyValues, cancellationToken);
        }

        public override int GetHashCode()
        {
            return _members.GetHashCode();
        }

        public override EntityEntry<Member> Remove(Member entity)
        {
            return _members.Remove(entity);
        }

        public override void RemoveRange(params Member[] entities)
        {
            _members.RemoveRange(entities);
        }

        public override void RemoveRange(IEnumerable<Member> entities)
        {
            _members.RemoveRange(entities);
        }

        public override string ToString()
        {
            return _members.ToString();
        }

        public override EntityEntry<Member> Update(Member entity)
        {
            return _members.Update(entity);
        }

        public override void UpdateRange(params Member[] entities)
        {
            _members.UpdateRange(entities);
        }

        public override void UpdateRange(IEnumerable<Member> entities)
        {
            _members.UpdateRange(entities);
        }
    }
}
