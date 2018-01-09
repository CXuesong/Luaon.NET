﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Luaon.Linq
{
    /// <summary>
    /// Represents a table in Lua.
    /// </summary>
    /// <remarks>See <a herf="http://www.lua.org/manual/5.3/manual.html#2.1">2.1 Values and Types</a> for the basic concepts on Lua tables.</remarks>
    public class LTable : LToken, ICollection<LField>
    {

        private readonly LTableStore store = new LTableStore();

        public LTable()
        {

        }

        public LTable(IEnumerable<object> content)
        {
            foreach (var i in content) Add(i);
        }

        public LTable(params object[] content)
        {
            foreach (var i in content) Add(i);
        }

        /// <summary>
        /// Inserts a field at the end of the table expression.
        /// </summary>
        /// <param name="field">The new field to be appended.</param>
        /// <exception cref="ArgumentNullException"><paramref name="field"/> is <c>null</c>.</exception>
        public void Add(LField field)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));
            store.Add(field);
        }

        /// <summary>
        /// Inserts a new positional field at the end of the table expression.
        /// </summary>
        /// <param name="value">The value of the new field to be appended. <c>null</c> will be treated the same as <see cref="LValue.Nil"/>.</param>
        public void Add(LToken value)
        {
            Add(null, value);
        }

        /// <summary>
        /// Inserts a field at the end of the table expression.
        /// </summary>
        /// <param name="name">The name of the new field to be appended.</param>
        /// <param name="value">The value of the new field to be appended. <c>null</c> will be treated the same as <see cref="LValue.Nil"/>.</param>
        public void Add(LValue name, LToken value)
        {
            store.Add(new LField(name, value));
        }

        /// <summary>
        /// Inserts a field or value at the end of the table expression.
        /// </summary>
        /// <param name="content">The field or value of the field.</param>
        public void Add(object content)
        {
            if (content is LField field)
            {
                Add(field);
            }
            else if (content is LToken value)
            {
                Add(value);
            }
            else
            {
                var tv = new LValue(content);
                Add(tv);
            }
        }

        /// <inheritdoc />
        public void Clear()
        {
            store.Clear();
        }

        /// <inheritdoc />
        bool ICollection<LField>.Contains(LField item)
        {
            return store.Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(LField[] array, int arrayIndex)
        {
            store.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public bool Remove(LField item)
        {
            return store.Remove(item);
        }

        /// <summary>
        /// Removes a field from table by its field name.
        /// </summary>
        /// <param name="name">Field name.</param>
        /// <returns>Whether a matching item has been removed.</returns>
        /// <remarks>To remvoe a positional field, pass a numeric value to the <paramref name="name"/> parameter.</remarks>
        public bool Remove(LValue name)
        {
            return store.RemoveByName(name);
        }

        /// <summary>
        /// Removes a field from table by its field name.
        /// </summary>
        /// <param name="name">Field name.</param>
        /// <returns>Whether a matching item has been removed.</returns>
        public bool Remove(int name)
        {
            // TODO remvoe by int, rather than LValue
            return store.RemoveByName(name);
        }

        /// <inheritdoc />
        public int Count => store.Count;

        /// <inheritdoc />
        bool ICollection<LField>.IsReadOnly => false;

        /// <summary>
        /// Inserts an element at the specified position <paramref name="index"/> in the table expression.
        /// </summary>
        /// <param name="index">The position to insert the value.</param>
        /// <param name="item">The new value to be inserted.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0 or greater than <see cref="Count"/>.</exception>
        public void Insert(int index, LField item)
        {
            store.Insert(index, item);
        }

        /// <summary>
        /// Removes the field element at the specified document position.
        /// </summary>
        /// <param name="index">The position to remove the value.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0 or greater than or equal to <see cref="Count"/>.</exception>
        /// <remarks>The index pos can also be 0 when #list is 0, or #list + 1; in those cases, the function erases the element list[pos].</remarks>
        public void RemoveAt(int index)
        {
            store.RemoveAt(index);
        }

        /// <summary>
        /// Gets/sets the field value associated with the specified field key.
        /// </summary>
        /// <param name="name">The key of the field to set or get.</param>
        /// <returns>The value of the specified field, or <c>null</c> if the field does not exist.</returns>
        public override LToken this[int name]
        {
            get { return store.FieldFromName(name, null, false)?.Value; }
            set { store.FieldFromName(name, null, true).Value = value; }
        }

        /// <summary>
        /// Gets/sets the field value associated with the specified field key.
        /// </summary>
        /// <param name="name">The key of the field to set or get.</param>
        /// <returns>The value of the specified field, or <c>null</c> if the field does not exist.</returns>
        public override LToken this[string name]
        {
            get { return store.FieldFromName(name, false)?.Value; }
            set { store.FieldFromName(name, true).Value = value; }
        }

        /// <summary>
        /// Gets/sets the field value associated with the specified field key.
        /// </summary>
        /// <param name="name">The key of the field to set or get.</param>
        /// <returns>The value of the specified field, or <c>null</c> if the field does not exist.</returns>
        public override LToken this[LValue name]
        {
            get { return store.FieldFromName(name, false)?.Value; }
            set { store.FieldFromName(name, true).Value = value; }
        }

        /// <summary>
        /// Tries to get the field instance with the specified field name.
        /// </summary>
        /// <param name="name">The field name.</param>
        /// <returns>The field instance, or <c>null</c> if no matching field exists.</returns>
        public LField Field(LValue name)
        {
            return store.FieldFromName(name, false);
        }

        /// <summary>
        /// Tries to get the field instance with the specified field name.
        /// </summary>
        /// <param name="name">The field name.</param>
        /// <returns>The field instance, or <c>null</c> if no matching field exists.</returns>
        public LField Field(int name)
        {
            return store.FieldFromName(name, null, false);
        }

        /// <summary>
        /// Tries to get the field instance with the specified field name.
        /// </summary>
        /// <param name="name">The field name.</param>
        /// <returns>The field instance, or <c>null</c> if no matching field exists.</returns>
        public LField Field(string name)
        {
            return store.FieldFromName(name, false);
        }

        /// <summary>
        /// Tries to get the field instance with the specified positional index.
        /// </summary>
        /// <param name="index">The in-document postion of the field.</param>
        /// <returns>The field instance.</returns>
        /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is less than 0 or greater than or equal to <see cref="Count"/>.</exception>
        public LField FieldAt(int index)
        {
            return store[index];
        }

        /// <summary>
        /// Gets a sequence containing all the names of the fields.
        /// </summary>
        public IEnumerable<LValue> FieldNames() => store.Names();

        /// <summary>
        /// Gets a sequence containing all the values of the fields.
        /// </summary>
        public IEnumerable<LToken> FieldValues() => store.Values();

        /// <inheritdoc />
        public override LTokenType TokenType => LTokenType.Table;

        /// <inheritdoc />
        public override void WriteTo(LuaTableTextWriter writer)
        {
            writer.WriteStartTable();
            foreach (var field in store)
            {
                field.WriteTo(writer);
            }
            writer.WriteEndTable();
        }

        /// <inheritdoc />
        public override LToken DeepClone()
        {
            var table = new LTable(store.Select(t => t.DeepClone()));
            return table;
        }

        /// <inheritdoc />
        public IEnumerator<LField> GetEnumerator()
        {
            return store.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}