using System;

namespace PhoneBookCommonLib
{
    /// <summary>
    /// Represents a record in phone book.
    /// </summary>
    [Serializable]
    public class PhoneBookRecord
    {
        /// <summary>
        /// Name of the person.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Phone number of the person.
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Creation date of this record.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Creates a new PhoneBookRecord object.
        /// </summary>
        public PhoneBookRecord()
        {
            CreationDate = DateTime.Now;
        }

        /// <summary>
        /// Generates a string representation of this object.
        /// </summary>
        /// <returns>String representation of this object</returns>
        public override string ToString()
        {
            return string.Format("Name = {0}, Phone = {1}", Name, Phone);
        }
    }
}