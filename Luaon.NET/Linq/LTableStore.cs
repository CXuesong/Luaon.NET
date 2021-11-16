using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Luaon.Linq
{
    /// <summary>
    /// Stores Lua table fields by document order.
    /// </summary>
    internal class LTableStore : Collection<LField>
    {

        private readonly Dictionary<LValue, LField> fieldsDict = new Dictionary<LValue, LField>();

        /// <inheritdoc />
        protected override void InsertItem(int index, LField item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (item.Name != null)
            {
                fieldsDict[item.Name] = item;
            }

            base.InsertItem(index, item);
        }

        /// <inheritdoc />
        protected override void SetItem(int index, LField item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            var oldField = Items[index];
            base.SetItem(index, item);
            if (oldField != item && oldField.Name != item.Name)
            {
                if (fieldsDict.TryGetValue(oldField.Name, out var itemInDict) && fieldsDict.Remove(oldField.Name))
                {
                    // There might be duplicate keys…
                    if (oldField != itemInDict) fieldsDict.Add(itemInDict.Name, itemInDict);
                }
                fieldsDict[item.Name] = item;
            }
        }

        /// <inheritdoc />
        protected override void RemoveItem(int index)
        {
            var oldField = Items[index];
            base.RemoveItem(index);
            if (fieldsDict.TryGetValue(oldField.Name, out var itemInDict) && fieldsDict.Remove(oldField.Name))
            {
                // There might be duplicate keys…
                if (oldField != itemInDict) fieldsDict.Add(itemInDict.Name, itemInDict);
            }
        }

        /// <inheritdoc />
        protected override void ClearItems()
        {
            fieldsDict.Clear();
            base.ClearItems();
        }

        public LField FieldFromName(int name, LValue nameRef, bool allowsCreation)
        {
            if (nameRef != null && fieldsDict.TryGetValue(name, out var field)) return field;

            if (fieldsDict.Count == 0)
            {
                // Positional fields only
                // Fast route
                if (name <= 0 || name > Items.Count)
                {
                    if (!allowsCreation) return null;
                    var f = new LField(name == Items.Count ? null : new LValue(name), null);
                    Items.Add(f);
                    return f;
                }

                return Items[name - 1];
            }

            // Positional field
            // Sanity check
            if (name <= 0 || name > Items.Count - fieldsDict.Count)
            {
                var nr = nameRef ?? new LValue(name);
                if (fieldsDict.TryGetValue(nr, out var f)) return f;
                if (!allowsCreation) return null;
                f = new LField(name > 0 && name - 1 == Items.Count - fieldsDict.Count ? null : nr, null);
                Items.Add(f);
                return f;
            }

            // Get the positional field
            var positionalIndex = 0;
            foreach (var i in Items)
            {
                if (i.Name != null) continue;
                positionalIndex++;
                if (name == positionalIndex) return i;
            }

            // Create the positional field
            if (allowsCreation)
            {
                // Use implicit index if possible.
                var f = name == positionalIndex + 1 ? new LField(LValue.Nil) : new LField(name, LValue.Nil);
                this.Add(f);
                return f;
            }

            return null;
        }

        public LField FieldFromName(LValue name, bool allowsCreation)
        {
            Debug.Assert(name != null && name.TokenType != LTokenType.Nil);
            if (name.TokenType == LTokenType.Integer) return FieldFromName((int)name, name, allowsCreation);
            // Named field
            if (!fieldsDict.TryGetValue(name, out var field))
            {
                if (!allowsCreation) return null;
                field = new LField(name, LValue.Nil);
                this.Add(field);
            }
            return field;
        }

        public bool RemoveByName(LValue name)
        {
            if (name == null || name.TokenType == LTokenType.Nil) return false;

            if (fieldsDict.Count == 0)
            {
                // Positional fields only
                // Fast route
                if (name.TokenType == LTokenType.Integer)
                {
                    var intName = (int)name;
                    if (intName <= 0 || intName > Items.Count) return false;
                    Items.RemoveAt(intName - 1);
                    return true;
                }
            }

            // Named field
            if (fieldsDict.TryGetValue(name, out var field))
            {
                var success = Items.Remove(field);
                Debug.Assert(success);
                return true;
            }

            // Positional field
            if (name.TokenType == LTokenType.Integer)
            {
                var intName = (int)name;
                // Sanity check
                if (intName <= 0 || intName > Items.Count) return false;
                int positionalIndex = 0;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Name == null)
                    {
                        positionalIndex++;
                        if (intName == positionalIndex)
                        {
                            Items.RemoveAt(i);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public IEnumerable<LValue> Names()
        {
            if (fieldsDict.Count == 0) return Enumerable.Range(1, Items.Count).Select(i => (LValue)i);
            return IterateNames();

            IEnumerable<LValue> IterateNames()
            {
                int positionalIndex = 0;
                foreach (var field in Items)
                {
                    if (field.Name != null)
                    {
                        yield return field.Name;
                    }
                    else
                    {
                        positionalIndex++;
                        yield return positionalIndex;
                    }
                }
            }
        }

        public IEnumerable<LToken> Values()
        {
            return Items.Select(i => i.Value);
        }


    }
}
