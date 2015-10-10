// Copyright 2013 Basarat Ali Syed. All Rights Reserved.
//
// Licensed under MIT open source license http://opensource.org/licenses/MIT
//
// Orginal javascript code was by Mauricio Santos
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
/**
 * @namespace Top level namespace for collections, a TypeScript data structure library.
 */
var collections;
(function (collections) {
    var _hasOwnProperty = Object.prototype.hasOwnProperty;
    var has = function (obj, prop) {
        return _hasOwnProperty.call(obj, prop);
    };
    /**
     * Default function to compare element order.
     * @function
     */
    function defaultCompare(a, b) {
        if (a < b) {
            return -1;
        }
        else if (a === b) {
            return 0;
        }
        else {
            return 1;
        }
    }
    collections.defaultCompare = defaultCompare;
    /**
     * Default function to test equality.
     * @function
     */
    function defaultEquals(a, b) {
        return a === b;
    }
    collections.defaultEquals = defaultEquals;
    /**
     * Default function to convert an object to a string.
     * @function
     */
    function defaultToString(item) {
        if (item === null) {
            return 'COLLECTION_NULL';
        }
        else if (collections.isUndefined(item)) {
            return 'COLLECTION_UNDEFINED';
        }
        else if (collections.isString(item)) {
            return '$s' + item;
        }
        else {
            return '$o' + item.toString();
        }
    }
    collections.defaultToString = defaultToString;
    /**
    * Joins all the properies of the object using the provided join string
    */
    function makeString(item, join) {
        if (join === void 0) { join = ","; }
        if (item === null) {
            return 'COLLECTION_NULL';
        }
        else if (collections.isUndefined(item)) {
            return 'COLLECTION_UNDEFINED';
        }
        else if (collections.isString(item)) {
            return item.toString();
        }
        else {
            var toret = "{";
            var first = true;
            for (var prop in item) {
                if (has(item, prop)) {
                    if (first)
                        first = false;
                    else
                        toret = toret + join;
                    toret = toret + prop + ":" + item[prop];
                }
            }
            return toret + "}";
        }
    }
    collections.makeString = makeString;
    /**
     * Checks if the given argument is a function.
     * @function
     */
    function isFunction(func) {
        return (typeof func) === 'function';
    }
    collections.isFunction = isFunction;
    /**
     * Checks if the given argument is undefined.
     * @function
     */
    function isUndefined(obj) {
        return (typeof obj) === 'undefined';
    }
    collections.isUndefined = isUndefined;
    /**
     * Checks if the given argument is a string.
     * @function
     */
    function isString(obj) {
        return Object.prototype.toString.call(obj) === '[object String]';
    }
    collections.isString = isString;
    /**
     * Reverses a compare function.
     * @function
     */
    function reverseCompareFunction(compareFunction) {
        if (!collections.isFunction(compareFunction)) {
            return function (a, b) {
                if (a < b) {
                    return 1;
                }
                else if (a === b) {
                    return 0;
                }
                else {
                    return -1;
                }
            };
        }
        else {
            return function (d, v) {
                return compareFunction(d, v) * -1;
            };
        }
    }
    collections.reverseCompareFunction = reverseCompareFunction;
    /**
     * Returns an equal function given a compare function.
     * @function
     */
    function compareToEquals(compareFunction) {
        return function (a, b) {
            return compareFunction(a, b) === 0;
        };
    }
    collections.compareToEquals = compareToEquals;
    /**
     * @namespace Contains various functions for manipulating arrays.
     */
    var arrays;
    (function (arrays) {
        /**
         * Returns the position of the first occurrence of the specified item
         * within the specified array.
         * @param {*} array the array in which to search the element.
         * @param {Object} item the element to search.
         * @param {function(Object,Object):boolean=} equalsFunction optional function used to
         * check equality between 2 elements.
         * @return {number} the position of the first occurrence of the specified element
         * within the specified array, or -1 if not found.
         */
        function indexOf(array, item, equalsFunction) {
            var equals = equalsFunction || collections.defaultEquals;
            var length = array.length;
            for (var i = 0; i < length; i++) {
                if (equals(array[i], item)) {
                    return i;
                }
            }
            return -1;
        }
        arrays.indexOf = indexOf;
        /**
         * Returns the position of the last occurrence of the specified element
         * within the specified array.
         * @param {*} array the array in which to search the element.
         * @param {Object} item the element to search.
         * @param {function(Object,Object):boolean=} equalsFunction optional function used to
         * check equality between 2 elements.
         * @return {number} the position of the last occurrence of the specified element
         * within the specified array or -1 if not found.
         */
        function lastIndexOf(array, item, equalsFunction) {
            var equals = equalsFunction || collections.defaultEquals;
            var length = array.length;
            for (var i = length - 1; i >= 0; i--) {
                if (equals(array[i], item)) {
                    return i;
                }
            }
            return -1;
        }
        arrays.lastIndexOf = lastIndexOf;
        /**
         * Returns true if the specified array contains the specified element.
         * @param {*} array the array in which to search the element.
         * @param {Object} item the element to search.
         * @param {function(Object,Object):boolean=} equalsFunction optional function to
         * check equality between 2 elements.
         * @return {boolean} true if the specified array contains the specified element.
         */
        function contains(array, item, equalsFunction) {
            return arrays.indexOf(array, item, equalsFunction) >= 0;
        }
        arrays.contains = contains;
        /**
         * Removes the first ocurrence of the specified element from the specified array.
         * @param {*} array the array in which to search element.
         * @param {Object} item the element to search.
         * @param {function(Object,Object):boolean=} equalsFunction optional function to
         * check equality between 2 elements.
         * @return {boolean} true if the array changed after this call.
         */
        function remove(array, item, equalsFunction) {
            var index = arrays.indexOf(array, item, equalsFunction);
            if (index < 0) {
                return false;
            }
            array.splice(index, 1);
            return true;
        }
        arrays.remove = remove;
        /**
         * Returns the number of elements in the specified array equal
         * to the specified object.
         * @param {Array} array the array in which to determine the frequency of the element.
         * @param {Object} item the element whose frequency is to be determined.
         * @param {function(Object,Object):boolean=} equalsFunction optional function used to
         * check equality between 2 elements.
         * @return {number} the number of elements in the specified array
         * equal to the specified object.
         */
        function frequency(array, item, equalsFunction) {
            var equals = equalsFunction || collections.defaultEquals;
            var length = array.length;
            var freq = 0;
            for (var i = 0; i < length; i++) {
                if (equals(array[i], item)) {
                    freq++;
                }
            }
            return freq;
        }
        arrays.frequency = frequency;
        /**
         * Returns true if the two specified arrays are equal to one another.
         * Two arrays are considered equal if both arrays contain the same number
         * of elements, and all corresponding pairs of elements in the two
         * arrays are equal and are in the same order.
         * @param {Array} array1 one array to be tested for equality.
         * @param {Array} array2 the other array to be tested for equality.
         * @param {function(Object,Object):boolean=} equalsFunction optional function used to
         * check equality between elemements in the arrays.
         * @return {boolean} true if the two arrays are equal
         */
        function equals(array1, array2, equalsFunction) {
            var equals = equalsFunction || collections.defaultEquals;
            if (array1.length !== array2.length) {
                return false;
            }
            var length = array1.length;
            for (var i = 0; i < length; i++) {
                if (!equals(array1[i], array2[i])) {
                    return false;
                }
            }
            return true;
        }
        arrays.equals = equals;
        /**
         * Returns shallow a copy of the specified array.
         * @param {*} array the array to copy.
         * @return {Array} a copy of the specified array
         */
        function copy(array) {
            return array.concat();
        }
        arrays.copy = copy;
        /**
         * Swaps the elements at the specified positions in the specified array.
         * @param {Array} array The array in which to swap elements.
         * @param {number} i the index of one element to be swapped.
         * @param {number} j the index of the other element to be swapped.
         * @return {boolean} true if the array is defined and the indexes are valid.
         */
        function swap(array, i, j) {
            if (i < 0 || i >= array.length || j < 0 || j >= array.length) {
                return false;
            }
            var temp = array[i];
            array[i] = array[j];
            array[j] = temp;
            return true;
        }
        arrays.swap = swap;
        function toString(array) {
            return '[' + array.toString() + ']';
        }
        arrays.toString = toString;
        /**
         * Executes the provided function once for each element present in this array
         * starting from index 0 to length - 1.
         * @param {Array} array The array in which to iterate.
         * @param {function(Object):*} callback function to execute, it is
         * invoked with one argument: the element value, to break the iteration you can
         * optionally return false.
         */
        function forEach(array, callback) {
            var lenght = array.length;
            for (var i = 0; i < lenght; i++) {
                if (callback(array[i]) === false) {
                    return;
                }
            }
        }
        arrays.forEach = forEach;
    })(arrays = collections.arrays || (collections.arrays = {}));
    var LinkedList = (function () {
        /**
        * Creates an empty Linked List.
        * @class A linked list is a data structure consisting of a group of nodes
        * which together represent a sequence.
        * @constructor
        */
        function LinkedList() {
            /**
            * First node in the list
            * @type {Object}
            * @private
            */
            this.firstNode = null;
            /**
            * Last node in the list
            * @type {Object}
            * @private
            */
            this.lastNode = null;
            /**
            * Number of elements in the list
            * @type {number}
            * @private
            */
            this.nElements = 0;
        }
        /**
        * Adds an element to this list.
        * @param {Object} item element to be added.
        * @param {number=} index optional index to add the element. If no index is specified
        * the element is added to the end of this list.
        * @return {boolean} true if the element was added or false if the index is invalid
        * or if the element is undefined.
        */
        LinkedList.prototype.add = function (item, index) {
            if (collections.isUndefined(index)) {
                index = this.nElements;
            }
            if (index < 0 || index > this.nElements || collections.isUndefined(item)) {
                return false;
            }
            var newNode = this.createNode(item);
            if (this.nElements === 0) {
                // First node in the list.
                this.firstNode = newNode;
                this.lastNode = newNode;
            }
            else if (index === this.nElements) {
                // Insert at the end.
                this.lastNode.next = newNode;
                this.lastNode = newNode;
            }
            else if (index === 0) {
                // Change first node.
                newNode.next = this.firstNode;
                this.firstNode = newNode;
            }
            else {
                var prev = this.nodeAtIndex(index - 1);
                newNode.next = prev.next;
                prev.next = newNode;
            }
            this.nElements++;
            return true;
        };
        /**
        * Returns the first element in this list.
        * @return {*} the first element of the list or undefined if the list is
        * empty.
        */
        LinkedList.prototype.first = function () {
            if (this.firstNode !== null) {
                return this.firstNode.element;
            }
            return undefined;
        };
        /**
        * Returns the last element in this list.
        * @return {*} the last element in the list or undefined if the list is
        * empty.
        */
        LinkedList.prototype.last = function () {
            if (this.lastNode !== null) {
                return this.lastNode.element;
            }
            return undefined;
        };
        /**
         * Returns the element at the specified position in this list.
         * @param {number} index desired index.
         * @return {*} the element at the given index or undefined if the index is
         * out of bounds.
         */
        LinkedList.prototype.elementAtIndex = function (index) {
            var node = this.nodeAtIndex(index);
            if (node === null) {
                return undefined;
            }
            return node.element;
        };
        /**
         * Returns the index in this list of the first occurrence of the
         * specified element, or -1 if the List does not contain this element.
         * <p>If the elements inside this list are
         * not comparable with the === operator a custom equals function should be
         * provided to perform searches, the function must receive two arguments and
         * return true if they are equal, false otherwise. Example:</p>
         *
         * <pre>
         * var petsAreEqualByName = function(pet1, pet2) {
         *  return pet1.name === pet2.name;
         * }
         * </pre>
         * @param {Object} item element to search for.
         * @param {function(Object,Object):boolean=} equalsFunction Optional
         * function used to check if two elements are equal.
         * @return {number} the index in this list of the first occurrence
         * of the specified element, or -1 if this list does not contain the
         * element.
         */
        LinkedList.prototype.indexOf = function (item, equalsFunction) {
            var equalsF = equalsFunction || collections.defaultEquals;
            if (collections.isUndefined(item)) {
                return -1;
            }
            var currentNode = this.firstNode;
            var index = 0;
            while (currentNode !== null) {
                if (equalsF(currentNode.element, item)) {
                    return index;
                }
                index++;
                currentNode = currentNode.next;
            }
            return -1;
        };
        /**
           * Returns true if this list contains the specified element.
           * <p>If the elements inside the list are
           * not comparable with the === operator a custom equals function should be
           * provided to perform searches, the function must receive two arguments and
           * return true if they are equal, false otherwise. Example:</p>
           *
           * <pre>
           * var petsAreEqualByName = function(pet1, pet2) {
           *  return pet1.name === pet2.name;
           * }
           * </pre>
           * @param {Object} item element to search for.
           * @param {function(Object,Object):boolean=} equalsFunction Optional
           * function used to check if two elements are equal.
           * @return {boolean} true if this list contains the specified element, false
           * otherwise.
           */
        LinkedList.prototype.contains = function (item, equalsFunction) {
            return (this.indexOf(item, equalsFunction) >= 0);
        };
        /**
         * Removes the first occurrence of the specified element in this list.
         * <p>If the elements inside the list are
         * not comparable with the === operator a custom equals function should be
         * provided to perform searches, the function must receive two arguments and
         * return true if they are equal, false otherwise. Example:</p>
         *
         * <pre>
         * var petsAreEqualByName = function(pet1, pet2) {
         *  return pet1.name === pet2.name;
         * }
         * </pre>
         * @param {Object} item element to be removed from this list, if present.
         * @return {boolean} true if the list contained the specified element.
         */
        LinkedList.prototype.remove = function (item, equalsFunction) {
            var equalsF = equalsFunction || collections.defaultEquals;
            if (this.nElements < 1 || collections.isUndefined(item)) {
                return false;
            }
            var previous = null;
            var currentNode = this.firstNode;
            while (currentNode !== null) {
                if (equalsF(currentNode.element, item)) {
                    if (currentNode === this.firstNode) {
                        this.firstNode = this.firstNode.next;
                        if (currentNode === this.lastNode) {
                            this.lastNode = null;
                        }
                    }
                    else if (currentNode === this.lastNode) {
                        this.lastNode = previous;
                        previous.next = currentNode.next;
                        currentNode.next = null;
                    }
                    else {
                        previous.next = currentNode.next;
                        currentNode.next = null;
                    }
                    this.nElements--;
                    return true;
                }
                previous = currentNode;
                currentNode = currentNode.next;
            }
            return false;
        };
        /**
         * Removes all of the elements from this list.
         */
        LinkedList.prototype.clear = function () {
            this.firstNode = null;
            this.lastNode = null;
            this.nElements = 0;
        };
        /**
         * Returns true if this list is equal to the given list.
         * Two lists are equal if they have the same elements in the same order.
         * @param {LinkedList} other the other list.
         * @param {function(Object,Object):boolean=} equalsFunction optional
         * function used to check if two elements are equal. If the elements in the lists
         * are custom objects you should provide a function, otherwise
         * the === operator is used to check equality between elements.
         * @return {boolean} true if this list is equal to the given list.
         */
        LinkedList.prototype.equals = function (other, equalsFunction) {
            var eqF = equalsFunction || collections.defaultEquals;
            if (!(other instanceof collections.LinkedList)) {
                return false;
            }
            if (this.size() !== other.size()) {
                return false;
            }
            return this.equalsAux(this.firstNode, other.firstNode, eqF);
        };
        /**
        * @private
        */
        LinkedList.prototype.equalsAux = function (n1, n2, eqF) {
            while (n1 !== null) {
                if (!eqF(n1.element, n2.element)) {
                    return false;
                }
                n1 = n1.next;
                n2 = n2.next;
            }
            return true;
        };
        /**
         * Removes the element at the specified position in this list.
         * @param {number} index given index.
         * @return {*} removed element or undefined if the index is out of bounds.
         */
        LinkedList.prototype.removeElementAtIndex = function (index) {
            if (index < 0 || index >= this.nElements) {
                return undefined;
            }
            var element;
            if (this.nElements === 1) {
                //First node in the list.
                element = this.firstNode.element;
                this.firstNode = null;
                this.lastNode = null;
            }
            else {
                var previous = this.nodeAtIndex(index - 1);
                if (previous === null) {
                    element = this.firstNode.element;
                    this.firstNode = this.firstNode.next;
                }
                else if (previous.next === this.lastNode) {
                    element = this.lastNode.element;
                    this.lastNode = previous;
                }
                if (previous !== null) {
                    element = previous.next.element;
                    previous.next = previous.next.next;
                }
            }
            this.nElements--;
            return element;
        };
        /**
         * Executes the provided function once for each element present in this list in order.
         * @param {function(Object):*} callback function to execute, it is
         * invoked with one argument: the element value, to break the iteration you can
         * optionally return false.
         */
        LinkedList.prototype.forEach = function (callback) {
            var currentNode = this.firstNode;
            while (currentNode !== null) {
                if (callback(currentNode.element) === false) {
                    break;
                }
                currentNode = currentNode.next;
            }
        };
        /**
         * Reverses the order of the elements in this linked list (makes the last
         * element first, and the first element last).
         */
        LinkedList.prototype.reverse = function () {
            var previous = null;
            var current = this.firstNode;
            var temp = null;
            while (current !== null) {
                temp = current.next;
                current.next = previous;
                previous = current;
                current = temp;
            }
            temp = this.firstNode;
            this.firstNode = this.lastNode;
            this.lastNode = temp;
        };
        /**
         * Returns an array containing all of the elements in this list in proper
         * sequence.
         * @return {Array.<*>} an array containing all of the elements in this list,
         * in proper sequence.
         */
        LinkedList.prototype.toArray = function () {
            var array = [];
            var currentNode = this.firstNode;
            while (currentNode !== null) {
                array.push(currentNode.element);
                currentNode = currentNode.next;
            }
            return array;
        };
        /**
         * Returns the number of elements in this list.
         * @return {number} the number of elements in this list.
         */
        LinkedList.prototype.size = function () {
            return this.nElements;
        };
        /**
         * Returns true if this list contains no elements.
         * @return {boolean} true if this list contains no elements.
         */
        LinkedList.prototype.isEmpty = function () {
            return this.nElements <= 0;
        };
        LinkedList.prototype.toString = function () {
            return collections.arrays.toString(this.toArray());
        };
        /**
         * @private
         */
        LinkedList.prototype.nodeAtIndex = function (index) {
            if (index < 0 || index >= this.nElements) {
                return null;
            }
            if (index === (this.nElements - 1)) {
                return this.lastNode;
            }
            var node = this.firstNode;
            for (var i = 0; i < index; i++) {
                node = node.next;
            }
            return node;
        };
        /**
         * @private
         */
        LinkedList.prototype.createNode = function (item) {
            return {
                element: item,
                next: null
            };
        };
        return LinkedList;
    })();
    collections.LinkedList = LinkedList; // End of linked list 
    var Dictionary = (function () {
        /**
         * Creates an empty dictionary.
         * @class <p>Dictionaries map keys to values; each key can map to at most one value.
         * This implementation accepts any kind of objects as keys.</p>
         *
         * <p>If the keys are custom objects a function which converts keys to unique
         * strings must be provided. Example:</p>
         * <pre>
         * function petToString(pet) {
         *  return pet.name;
         * }
         * </pre>
         * @constructor
         * @param {function(Object):string=} toStrFunction optional function used
         * to convert keys to strings. If the keys aren't strings or if toString()
         * is not appropriate, a custom function which receives a key and returns a
         * unique string must be provided.
         */
        function Dictionary(toStrFunction) {
            this.table = {};
            this.nElements = 0;
            this.toStr = toStrFunction || collections.defaultToString;
        }
        /**
         * Returns the value to which this dictionary maps the specified key.
         * Returns undefined if this dictionary contains no mapping for this key.
         * @param {Object} key key whose associated value is to be returned.
         * @return {*} the value to which this dictionary maps the specified key or
         * undefined if the map contains no mapping for this key.
         */
        Dictionary.prototype.getValue = function (key) {
            var pair = this.table['$' + this.toStr(key)];
            if (collections.isUndefined(pair)) {
                return undefined;
            }
            return pair.value;
        };
        /**
         * Associates the specified value with the specified key in this dictionary.
         * If the dictionary previously contained a mapping for this key, the old
         * value is replaced by the specified value.
         * @param {Object} key key with which the specified value is to be
         * associated.
         * @param {Object} value value to be associated with the specified key.
         * @return {*} previous value associated with the specified key, or undefined if
         * there was no mapping for the key or if the key/value are undefined.
         */
        Dictionary.prototype.setValue = function (key, value) {
            if (collections.isUndefined(key) || collections.isUndefined(value)) {
                return undefined;
            }
            var ret;
            var k = '$' + this.toStr(key);
            var previousElement = this.table[k];
            if (collections.isUndefined(previousElement)) {
                this.nElements++;
                ret = undefined;
            }
            else {
                ret = previousElement.value;
            }
            this.table[k] = {
                key: key,
                value: value
            };
            return ret;
        };
        /**
         * Removes the mapping for this key from this dictionary if it is present.
         * @param {Object} key key whose mapping is to be removed from the
         * dictionary.
         * @return {*} previous value associated with specified key, or undefined if
         * there was no mapping for key.
         */
        Dictionary.prototype.remove = function (key) {
            var k = '$' + this.toStr(key);
            var previousElement = this.table[k];
            if (!collections.isUndefined(previousElement)) {
                delete this.table[k];
                this.nElements--;
                return previousElement.value;
            }
            return undefined;
        };
        /**
         * Returns an array containing all of the keys in this dictionary.
         * @return {Array} an array containing all of the keys in this dictionary.
         */
        Dictionary.prototype.keys = function () {
            var array = [];
            for (var name in this.table) {
                if (has(this.table, name)) {
                    var pair = this.table[name];
                    array.push(pair.key);
                }
            }
            return array;
        };
        /**
         * Returns an array containing all of the values in this dictionary.
         * @return {Array} an array containing all of the values in this dictionary.
         */
        Dictionary.prototype.values = function () {
            var array = [];
            for (var name in this.table) {
                if (has(this.table, name)) {
                    var pair = this.table[name];
                    array.push(pair.value);
                }
            }
            return array;
        };
        /**
        * Executes the provided function once for each key-value pair
        * present in this dictionary.
        * @param {function(Object,Object):*} callback function to execute, it is
        * invoked with two arguments: key and value. To break the iteration you can
        * optionally return false.
        */
        Dictionary.prototype.forEach = function (callback) {
            for (var name in this.table) {
                if (has(this.table, name)) {
                    var pair = this.table[name];
                    var ret = callback(pair.key, pair.value);
                    if (ret === false) {
                        return;
                    }
                }
            }
        };
        /**
         * Returns true if this dictionary contains a mapping for the specified key.
         * @param {Object} key key whose presence in this dictionary is to be
         * tested.
         * @return {boolean} true if this dictionary contains a mapping for the
         * specified key.
         */
        Dictionary.prototype.containsKey = function (key) {
            return !collections.isUndefined(this.getValue(key));
        };
        /**
        * Removes all mappings from this dictionary.
        * @this {collections.Dictionary}
        */
        Dictionary.prototype.clear = function () {
            this.table = {};
            this.nElements = 0;
        };
        /**
         * Returns the number of keys in this dictionary.
         * @return {number} the number of key-value mappings in this dictionary.
         */
        Dictionary.prototype.size = function () {
            return this.nElements;
        };
        /**
         * Returns true if this dictionary contains no mappings.
         * @return {boolean} true if this dictionary contains no mappings.
         */
        Dictionary.prototype.isEmpty = function () {
            return this.nElements <= 0;
        };
        Dictionary.prototype.toString = function () {
            var toret = "{";
            this.forEach(function (k, v) {
                toret = toret + "\n\t" + k.toString() + " : " + v.toString();
            });
            return toret + "\n}";
        };
        return Dictionary;
    })();
    collections.Dictionary = Dictionary; // End of dictionary
    /**
     * This class is used by the LinkedDictionary Internally
     * Has to be a class, not an interface, because it needs to have
     * the 'unlink' function defined.
     */
    var LinkedDictionaryPair = (function () {
        function LinkedDictionaryPair(key, value) {
            this.key = key;
            this.value = value;
        }
        LinkedDictionaryPair.prototype.unlink = function () {
            this.prev.next = this.next;
            this.next.prev = this.prev;
        };
        return LinkedDictionaryPair;
    })();
    var LinkedDictionary = (function (_super) {
        __extends(LinkedDictionary, _super);
        function LinkedDictionary(toStrFunction) {
            _super.call(this, toStrFunction);
            this.head = new LinkedDictionaryPair(null, null);
            this.tail = new LinkedDictionaryPair(null, null);
            this.head.next = this.tail;
            this.tail.prev = this.head;
        }
        /**
         * Inserts the new node to the 'tail' of the list, updating the
         * neighbors, and moving 'this.tail' (the End of List indicator) that
         * to the end.
         */
        LinkedDictionary.prototype.appendToTail = function (entry) {
            var lastNode = this.tail.prev;
            lastNode.next = entry;
            entry.prev = lastNode;
            entry.next = this.tail;
            this.tail.prev = entry;
        };
        /**
         * Retrieves a linked dictionary from the table internally
         */
        LinkedDictionary.prototype.getLinkedDictionaryPair = function (key) {
            if (collections.isUndefined(key)) {
                return undefined;
            }
            var k = '$' + this.toStr(key);
            var pair = (this.table[k]);
            return pair;
        };
        /**
         * Returns the value to which this dictionary maps the specified key.
         * Returns undefined if this dictionary contains no mapping for this key.
         * @param {Object} key key whose associated value is to be returned.
         * @return {*} the value to which this dictionary maps the specified key or
         * undefined if the map contains no mapping for this key.
         */
        LinkedDictionary.prototype.getValue = function (key) {
            var pair = this.getLinkedDictionaryPair(key);
            if (!collections.isUndefined(pair)) {
                return pair.value;
            }
            return undefined;
        };
        /**
         * Removes the mapping for this key from this dictionary if it is present.
         * Also, if a value is present for this key, the entry is removed from the
         * insertion ordering.
         * @param {Object} key key whose mapping is to be removed from the
         * dictionary.
         * @return {*} previous value associated with specified key, or undefined if
         * there was no mapping for key.
         */
        LinkedDictionary.prototype.remove = function (key) {
            var pair = this.getLinkedDictionaryPair(key);
            if (!collections.isUndefined(pair)) {
                _super.prototype.remove.call(this, key); // This will remove it from the table
                pair.unlink(); // This will unlink it from the chain
                return pair.value;
            }
            return undefined;
        };
        /**
        * Removes all mappings from this LinkedDictionary.
        * @this {collections.LinkedDictionary}
        */
        LinkedDictionary.prototype.clear = function () {
            _super.prototype.clear.call(this);
            this.head.next = this.tail;
            this.tail.prev = this.head;
        };
        /**
         * Internal function used when updating an existing KeyValue pair.
         * It places the new value indexed by key into the table, but maintains
         * its place in the linked ordering.
         */
        LinkedDictionary.prototype.replace = function (oldPair, newPair) {
            var k = '$' + this.toStr(newPair.key);
            // set the new Pair's links to existingPair's links
            newPair.next = oldPair.next;
            newPair.prev = oldPair.prev;
            // Delete Existing Pair from the table, unlink it from chain.
            // As a result, the nElements gets decremented by this operation
            this.remove(oldPair.key);
            // Link new Pair in place of where oldPair was,
            // by pointing the old pair's neighbors to it.
            newPair.prev.next = newPair;
            newPair.next.prev = newPair;
            this.table[k] = newPair;
            // To make up for the fact that the number of elements was decremented,
            // We need to increase it by one.
            ++this.nElements;
        };
        /**
         * Associates the specified value with the specified key in this dictionary.
         * If the dictionary previously contained a mapping for this key, the old
         * value is replaced by the specified value.
         * Updating of a key that already exists maintains its place in the
         * insertion order into the map.
         * @param {Object} key key with which the specified value is to be
         * associated.
         * @param {Object} value value to be associated with the specified key.
         * @return {*} previous value associated with the specified key, or undefined if
         * there was no mapping for the key or if the key/value are undefined.
         */
        LinkedDictionary.prototype.setValue = function (key, value) {
            if (collections.isUndefined(key) || collections.isUndefined(value)) {
                return undefined;
            }
            var existingPair = this.getLinkedDictionaryPair(key);
            var newPair = new LinkedDictionaryPair(key, value);
            var k = '$' + this.toStr(key);
            // If there is already an element for that key, we 
            // keep it's place in the LinkedList
            if (!collections.isUndefined(existingPair)) {
                this.replace(existingPair, newPair);
                return existingPair.value;
            }
            else {
                this.appendToTail(newPair);
                this.table[k] = newPair;
                ++this.nElements;
                return undefined;
            }
        };
        /**
         * Returns an array containing all of the keys in this LinkedDictionary, ordered
         * by insertion order.
         * @return {Array} an array containing all of the keys in this LinkedDictionary,
         * ordered by insertion order.
         */
        LinkedDictionary.prototype.keys = function () {
            var array = [];
            this.forEach(function (key, value) {
                array.push(key);
            });
            return array;
        };
        /**
         * Returns an array containing all of the values in this LinkedDictionary, ordered by
         * insertion order.
         * @return {Array} an array containing all of the values in this LinkedDictionary,
         * ordered by insertion order.
         */
        LinkedDictionary.prototype.values = function () {
            var array = [];
            this.forEach(function (key, value) {
                array.push(value);
            });
            return array;
        };
        /**
        * Executes the provided function once for each key-value pair
        * present in this LinkedDictionary. It is done in the order of insertion
        * into the LinkedDictionary
        * @param {function(Object,Object):*} callback function to execute, it is
        * invoked with two arguments: key and value. To break the iteration you can
        * optionally return false.
        */
        LinkedDictionary.prototype.forEach = function (callback) {
            var crawlNode = this.head.next;
            while (crawlNode.next != null) {
                var ret = callback(crawlNode.key, crawlNode.value);
                if (ret === false) {
                    return;
                }
                crawlNode = crawlNode.next;
            }
        };
        return LinkedDictionary;
    })(Dictionary);
    collections.LinkedDictionary = LinkedDictionary; // End of LinkedDictionary
    // /**
    //  * Returns true if this dictionary is equal to the given dictionary.
    //  * Two dictionaries are equal if they contain the same mappings.
    //  * @param {collections.Dictionary} other the other dictionary.
    //  * @param {function(Object,Object):boolean=} valuesEqualFunction optional
    //  * function used to check if two values are equal.
    //  * @return {boolean} true if this dictionary is equal to the given dictionary.
    //  */
    // collections.Dictionary.prototype.equals = function(other,valuesEqualFunction) {
    // 	var eqF = valuesEqualFunction || collections.defaultEquals;
    // 	if(!(other instanceof collections.Dictionary)){
    // 		return false;
    // 	}
    // 	if(this.size() !== other.size()){
    // 		return false;
    // 	}
    // 	return this.equalsAux(this.firstNode,other.firstNode,eqF);
    // }
    var MultiDictionary = (function () {
        /**
         * Creates an empty multi dictionary.
         * @class <p>A multi dictionary is a special kind of dictionary that holds
         * multiple values against each key. Setting a value into the dictionary will
         * add the value to an array at that key. Getting a key will return an array,
         * holding all the values set to that key.
         * You can configure to allow duplicates in the values.
         * This implementation accepts any kind of objects as keys.</p>
         *
         * <p>If the keys are custom objects a function which converts keys to strings must be
         * provided. Example:</p>
         *
         * <pre>
         * function petToString(pet) {
           *  return pet.name;
           * }
         * </pre>
         * <p>If the values are custom objects a function to check equality between values
         * must be provided. Example:</p>
         *
         * <pre>
         * function petsAreEqualByAge(pet1,pet2) {
           *  return pet1.age===pet2.age;
           * }
         * </pre>
         * @constructor
         * @param {function(Object):string=} toStrFunction optional function
         * to convert keys to strings. If the keys aren't strings or if toString()
         * is not appropriate, a custom function which receives a key and returns a
         * unique string must be provided.
         * @param {function(Object,Object):boolean=} valuesEqualsFunction optional
         * function to check if two values are equal.
         *
         * @param allowDuplicateValues
         */
        function MultiDictionary(toStrFunction, valuesEqualsFunction, allowDuplicateValues) {
            if (allowDuplicateValues === void 0) { allowDuplicateValues = false; }
            this.dict = new Dictionary(toStrFunction);
            this.equalsF = valuesEqualsFunction || collections.defaultEquals;
            this.allowDuplicate = allowDuplicateValues;
        }
        /**
        * Returns an array holding the values to which this dictionary maps
        * the specified key.
        * Returns an empty array if this dictionary contains no mappings for this key.
        * @param {Object} key key whose associated values are to be returned.
        * @return {Array} an array holding the values to which this dictionary maps
        * the specified key.
        */
        MultiDictionary.prototype.getValue = function (key) {
            var values = this.dict.getValue(key);
            if (collections.isUndefined(values)) {
                return [];
            }
            return collections.arrays.copy(values);
        };
        /**
         * Adds the value to the array associated with the specified key, if
         * it is not already present.
         * @param {Object} key key with which the specified value is to be
         * associated.
         * @param {Object} value the value to add to the array at the key
         * @return {boolean} true if the value was not already associated with that key.
         */
        MultiDictionary.prototype.setValue = function (key, value) {
            if (collections.isUndefined(key) || collections.isUndefined(value)) {
                return false;
            }
            if (!this.containsKey(key)) {
                this.dict.setValue(key, [value]);
                return true;
            }
            var array = this.dict.getValue(key);
            if (!this.allowDuplicate) {
                if (collections.arrays.contains(array, value, this.equalsF)) {
                    return false;
                }
            }
            array.push(value);
            return true;
        };
        /**
         * Removes the specified values from the array of values associated with the
         * specified key. If a value isn't given, all values associated with the specified
         * key are removed.
         * @param {Object} key key whose mapping is to be removed from the
         * dictionary.
         * @param {Object=} value optional argument to specify the value to remove
         * from the array associated with the specified key.
         * @return {*} true if the dictionary changed, false if the key doesn't exist or
         * if the specified value isn't associated with the specified key.
         */
        MultiDictionary.prototype.remove = function (key, value) {
            if (collections.isUndefined(value)) {
                var v = this.dict.remove(key);
                return !collections.isUndefined(v);
            }
            var array = this.dict.getValue(key);
            if (collections.arrays.remove(array, value, this.equalsF)) {
                if (array.length === 0) {
                    this.dict.remove(key);
                }
                return true;
            }
            return false;
        };
        /**
         * Returns an array containing all of the keys in this dictionary.
         * @return {Array} an array containing all of the keys in this dictionary.
         */
        MultiDictionary.prototype.keys = function () {
            return this.dict.keys();
        };
        /**
         * Returns an array containing all of the values in this dictionary.
         * @return {Array} an array containing all of the values in this dictionary.
         */
        MultiDictionary.prototype.values = function () {
            var values = this.dict.values();
            var array = [];
            for (var i = 0; i < values.length; i++) {
                var v = values[i];
                for (var j = 0; j < v.length; j++) {
                    array.push(v[j]);
                }
            }
            return array;
        };
        /**
         * Returns true if this dictionary at least one value associatted the specified key.
         * @param {Object} key key whose presence in this dictionary is to be
         * tested.
         * @return {boolean} true if this dictionary at least one value associatted
         * the specified key.
         */
        MultiDictionary.prototype.containsKey = function (key) {
            return this.dict.containsKey(key);
        };
        /**
         * Removes all mappings from this dictionary.
         */
        MultiDictionary.prototype.clear = function () {
            this.dict.clear();
        };
        /**
         * Returns the number of keys in this dictionary.
         * @return {number} the number of key-value mappings in this dictionary.
         */
        MultiDictionary.prototype.size = function () {
            return this.dict.size();
        };
        /**
         * Returns true if this dictionary contains no mappings.
         * @return {boolean} true if this dictionary contains no mappings.
         */
        MultiDictionary.prototype.isEmpty = function () {
            return this.dict.isEmpty();
        };
        return MultiDictionary;
    })();
    collections.MultiDictionary = MultiDictionary; // end of multi dictionary 
    var Heap = (function () {
        /**
         * Creates an empty Heap.
         * @class
         * <p>A heap is a binary tree, where the nodes maintain the heap property:
         * each node is smaller than each of its children and therefore a MinHeap
         * This implementation uses an array to store elements.</p>
         * <p>If the inserted elements are custom objects a compare function must be provided,
         *  at construction time, otherwise the <=, === and >= operators are
         * used to compare elements. Example:</p>
         *
         * <pre>
         * function compare(a, b) {
         *  if (a is less than b by some ordering criterion) {
         *     return -1;
         *  } if (a is greater than b by the ordering criterion) {
         *     return 1;
         *  }
         *  // a must be equal to b
         *  return 0;
         * }
         * </pre>
         *
         * <p>If a Max-Heap is wanted (greater elements on top) you can a provide a
         * reverse compare function to accomplish that behavior. Example:</p>
         *
         * <pre>
         * function reverseCompare(a, b) {
         *  if (a is less than b by some ordering criterion) {
         *     return 1;
         *  } if (a is greater than b by the ordering criterion) {
         *     return -1;
         *  }
         *  // a must be equal to b
         *  return 0;
         * }
         * </pre>
         *
         * @constructor
         * @param {function(Object,Object):number=} compareFunction optional
         * function used to compare two elements. Must return a negative integer,
         * zero, or a positive integer as the first argument is less than, equal to,
         * or greater than the second.
         */
        function Heap(compareFunction) {
            /**
             * Array used to store the elements od the heap.
             * @type {Array.<Object>}
             * @private
             */
            this.data = [];
            this.compare = compareFunction || collections.defaultCompare;
        }
        /**
         * Returns the index of the left child of the node at the given index.
         * @param {number} nodeIndex The index of the node to get the left child
         * for.
         * @return {number} The index of the left child.
         * @private
         */
        Heap.prototype.leftChildIndex = function (nodeIndex) {
            return (2 * nodeIndex) + 1;
        };
        /**
         * Returns the index of the right child of the node at the given index.
         * @param {number} nodeIndex The index of the node to get the right child
         * for.
         * @return {number} The index of the right child.
         * @private
         */
        Heap.prototype.rightChildIndex = function (nodeIndex) {
            return (2 * nodeIndex) + 2;
        };
        /**
         * Returns the index of the parent of the node at the given index.
         * @param {number} nodeIndex The index of the node to get the parent for.
         * @return {number} The index of the parent.
         * @private
         */
        Heap.prototype.parentIndex = function (nodeIndex) {
            return Math.floor((nodeIndex - 1) / 2);
        };
        /**
         * Returns the index of the smaller child node (if it exists).
         * @param {number} leftChild left child index.
         * @param {number} rightChild right child index.
         * @return {number} the index with the minimum value or -1 if it doesn't
         * exists.
         * @private
         */
        Heap.prototype.minIndex = function (leftChild, rightChild) {
            if (rightChild >= this.data.length) {
                if (leftChild >= this.data.length) {
                    return -1;
                }
                else {
                    return leftChild;
                }
            }
            else {
                if (this.compare(this.data[leftChild], this.data[rightChild]) <= 0) {
                    return leftChild;
                }
                else {
                    return rightChild;
                }
            }
        };
        /**
         * Moves the node at the given index up to its proper place in the heap.
         * @param {number} index The index of the node to move up.
         * @private
         */
        Heap.prototype.siftUp = function (index) {
            var parent = this.parentIndex(index);
            while (index > 0 && this.compare(this.data[parent], this.data[index]) > 0) {
                collections.arrays.swap(this.data, parent, index);
                index = parent;
                parent = this.parentIndex(index);
            }
        };
        /**
         * Moves the node at the given index down to its proper place in the heap.
         * @param {number} nodeIndex The index of the node to move down.
         * @private
         */
        Heap.prototype.siftDown = function (nodeIndex) {
            //smaller child index
            var min = this.minIndex(this.leftChildIndex(nodeIndex), this.rightChildIndex(nodeIndex));
            while (min >= 0 && this.compare(this.data[nodeIndex], this.data[min]) > 0) {
                collections.arrays.swap(this.data, min, nodeIndex);
                nodeIndex = min;
                min = this.minIndex(this.leftChildIndex(nodeIndex), this.rightChildIndex(nodeIndex));
            }
        };
        /**
         * Retrieves but does not remove the root element of this heap.
         * @return {*} The value at the root of the heap. Returns undefined if the
         * heap is empty.
         */
        Heap.prototype.peek = function () {
            if (this.data.length > 0) {
                return this.data[0];
            }
            else {
                return undefined;
            }
        };
        /**
         * Adds the given element into the heap.
         * @param {*} element the element.
         * @return true if the element was added or fals if it is undefined.
         */
        Heap.prototype.add = function (element) {
            if (collections.isUndefined(element)) {
                return undefined;
            }
            this.data.push(element);
            this.siftUp(this.data.length - 1);
            return true;
        };
        /**
         * Retrieves and removes the root element of this heap.
         * @return {*} The value removed from the root of the heap. Returns
         * undefined if the heap is empty.
         */
        Heap.prototype.removeRoot = function () {
            if (this.data.length > 0) {
                var obj = this.data[0];
                this.data[0] = this.data[this.data.length - 1];
                this.data.splice(this.data.length - 1, 1);
                if (this.data.length > 0) {
                    this.siftDown(0);
                }
                return obj;
            }
            return undefined;
        };
        /**
         * Returns true if this heap contains the specified element.
         * @param {Object} element element to search for.
         * @return {boolean} true if this Heap contains the specified element, false
         * otherwise.
         */
        Heap.prototype.contains = function (element) {
            var equF = collections.compareToEquals(this.compare);
            return collections.arrays.contains(this.data, element, equF);
        };
        /**
         * Returns the number of elements in this heap.
         * @return {number} the number of elements in this heap.
         */
        Heap.prototype.size = function () {
            return this.data.length;
        };
        /**
         * Checks if this heap is empty.
         * @return {boolean} true if and only if this heap contains no items; false
         * otherwise.
         */
        Heap.prototype.isEmpty = function () {
            return this.data.length <= 0;
        };
        /**
         * Removes all of the elements from this heap.
         */
        Heap.prototype.clear = function () {
            this.data.length = 0;
        };
        /**
         * Executes the provided function once for each element present in this heap in
         * no particular order.
         * @param {function(Object):*} callback function to execute, it is
         * invoked with one argument: the element value, to break the iteration you can
         * optionally return false.
         */
        Heap.prototype.forEach = function (callback) {
            collections.arrays.forEach(this.data, callback);
        };
        return Heap;
    })();
    collections.Heap = Heap;
    var Stack = (function () {
        /**
         * Creates an empty Stack.
         * @class A Stack is a Last-In-First-Out (LIFO) data structure, the last
         * element added to the stack will be the first one to be removed. This
         * implementation uses a linked list as a container.
         * @constructor
         */
        function Stack() {
            this.list = new LinkedList();
        }
        /**
         * Pushes an item onto the top of this stack.
         * @param {Object} elem the element to be pushed onto this stack.
         * @return {boolean} true if the element was pushed or false if it is undefined.
         */
        Stack.prototype.push = function (elem) {
            return this.list.add(elem, 0);
        };
        /**
         * Pushes an item onto the top of this stack.
         * @param {Object} elem the element to be pushed onto this stack.
         * @return {boolean} true if the element was pushed or false if it is undefined.
         */
        Stack.prototype.add = function (elem) {
            return this.list.add(elem, 0);
        };
        /**
         * Removes the object at the top of this stack and returns that object.
         * @return {*} the object at the top of this stack or undefined if the
         * stack is empty.
         */
        Stack.prototype.pop = function () {
            return this.list.removeElementAtIndex(0);
        };
        /**
         * Looks at the object at the top of this stack without removing it from the
         * stack.
         * @return {*} the object at the top of this stack or undefined if the
         * stack is empty.
         */
        Stack.prototype.peek = function () {
            return this.list.first();
        };
        /**
         * Returns the number of elements in this stack.
         * @return {number} the number of elements in this stack.
         */
        Stack.prototype.size = function () {
            return this.list.size();
        };
        /**
         * Returns true if this stack contains the specified element.
         * <p>If the elements inside this stack are
         * not comparable with the === operator, a custom equals function should be
         * provided to perform searches, the function must receive two arguments and
         * return true if they are equal, false otherwise. Example:</p>
         *
         * <pre>
         * var petsAreEqualByName (pet1, pet2) {
         *  return pet1.name === pet2.name;
         * }
         * </pre>
         * @param {Object} elem element to search for.
         * @param {function(Object,Object):boolean=} equalsFunction optional
         * function to check if two elements are equal.
         * @return {boolean} true if this stack contains the specified element,
         * false otherwise.
         */
        Stack.prototype.contains = function (elem, equalsFunction) {
            return this.list.contains(elem, equalsFunction);
        };
        /**
         * Checks if this stack is empty.
         * @return {boolean} true if and only if this stack contains no items; false
         * otherwise.
         */
        Stack.prototype.isEmpty = function () {
            return this.list.isEmpty();
        };
        /**
         * Removes all of the elements from this stack.
         */
        Stack.prototype.clear = function () {
            this.list.clear();
        };
        /**
         * Executes the provided function once for each element present in this stack in
         * LIFO order.
         * @param {function(Object):*} callback function to execute, it is
         * invoked with one argument: the element value, to break the iteration you can
         * optionally return false.
         */
        Stack.prototype.forEach = function (callback) {
            this.list.forEach(callback);
        };
        return Stack;
    })();
    collections.Stack = Stack; // End of stack 
    var Queue = (function () {
        /**
         * Creates an empty queue.
         * @class A queue is a First-In-First-Out (FIFO) data structure, the first
         * element added to the queue will be the first one to be removed. This
         * implementation uses a linked list as a container.
         * @constructor
         */
        function Queue() {
            this.list = new LinkedList();
        }
        /**
         * Inserts the specified element into the end of this queue.
         * @param {Object} elem the element to insert.
         * @return {boolean} true if the element was inserted, or false if it is undefined.
         */
        Queue.prototype.enqueue = function (elem) {
            return this.list.add(elem);
        };
        /**
         * Inserts the specified element into the end of this queue.
         * @param {Object} elem the element to insert.
         * @return {boolean} true if the element was inserted, or false if it is undefined.
         */
        Queue.prototype.add = function (elem) {
            return this.list.add(elem);
        };
        /**
         * Retrieves and removes the head of this queue.
         * @return {*} the head of this queue, or undefined if this queue is empty.
         */
        Queue.prototype.dequeue = function () {
            if (this.list.size() !== 0) {
                var el = this.list.first();
                this.list.removeElementAtIndex(0);
                return el;
            }
            return undefined;
        };
        /**
         * Retrieves, but does not remove, the head of this queue.
         * @return {*} the head of this queue, or undefined if this queue is empty.
         */
        Queue.prototype.peek = function () {
            if (this.list.size() !== 0) {
                return this.list.first();
            }
            return undefined;
        };
        /**
         * Returns the number of elements in this queue.
         * @return {number} the number of elements in this queue.
         */
        Queue.prototype.size = function () {
            return this.list.size();
        };
        /**
         * Returns true if this queue contains the specified element.
         * <p>If the elements inside this stack are
         * not comparable with the === operator, a custom equals function should be
         * provided to perform searches, the function must receive two arguments and
         * return true if they are equal, false otherwise. Example:</p>
         *
         * <pre>
         * var petsAreEqualByName (pet1, pet2) {
         *  return pet1.name === pet2.name;
         * }
         * </pre>
         * @param {Object} elem element to search for.
         * @param {function(Object,Object):boolean=} equalsFunction optional
         * function to check if two elements are equal.
         * @return {boolean} true if this queue contains the specified element,
         * false otherwise.
         */
        Queue.prototype.contains = function (elem, equalsFunction) {
            return this.list.contains(elem, equalsFunction);
        };
        /**
         * Checks if this queue is empty.
         * @return {boolean} true if and only if this queue contains no items; false
         * otherwise.
         */
        Queue.prototype.isEmpty = function () {
            return this.list.size() <= 0;
        };
        /**
         * Removes all of the elements from this queue.
         */
        Queue.prototype.clear = function () {
            this.list.clear();
        };
        /**
         * Executes the provided function once for each element present in this queue in
         * FIFO order.
         * @param {function(Object):*} callback function to execute, it is
         * invoked with one argument: the element value, to break the iteration you can
         * optionally return false.
         */
        Queue.prototype.forEach = function (callback) {
            this.list.forEach(callback);
        };
        return Queue;
    })();
    collections.Queue = Queue; // End of queue
    var PriorityQueue = (function () {
        /**
         * Creates an empty priority queue.
         * @class <p>In a priority queue each element is associated with a "priority",
         * elements are dequeued in highest-priority-first order (the elements with the
         * highest priority are dequeued first). Priority Queues are implemented as heaps.
         * If the inserted elements are custom objects a compare function must be provided,
         * otherwise the <=, === and >= operators are used to compare object priority.</p>
         * <pre>
         * function compare(a, b) {
         *  if (a is less than b by some ordering criterion) {
         *     return -1;
         *  } if (a is greater than b by the ordering criterion) {
         *     return 1;
         *  }
         *  // a must be equal to b
         *  return 0;
         * }
         * </pre>
         * @constructor
         * @param {function(Object,Object):number=} compareFunction optional
         * function used to compare two element priorities. Must return a negative integer,
         * zero, or a positive integer as the first argument is less than, equal to,
         * or greater than the second.
         */
        function PriorityQueue(compareFunction) {
            this.heap = new Heap(collections.reverseCompareFunction(compareFunction));
        }
        /**
         * Inserts the specified element into this priority queue.
         * @param {Object} element the element to insert.
         * @return {boolean} true if the element was inserted, or false if it is undefined.
         */
        PriorityQueue.prototype.enqueue = function (element) {
            return this.heap.add(element);
        };
        /**
         * Inserts the specified element into this priority queue.
         * @param {Object} element the element to insert.
         * @return {boolean} true if the element was inserted, or false if it is undefined.
         */
        PriorityQueue.prototype.add = function (element) {
            return this.heap.add(element);
        };
        /**
         * Retrieves and removes the highest priority element of this queue.
         * @return {*} the the highest priority element of this queue,
         *  or undefined if this queue is empty.
         */
        PriorityQueue.prototype.dequeue = function () {
            if (this.heap.size() !== 0) {
                var el = this.heap.peek();
                this.heap.removeRoot();
                return el;
            }
            return undefined;
        };
        /**
         * Retrieves, but does not remove, the highest priority element of this queue.
         * @return {*} the highest priority element of this queue, or undefined if this queue is empty.
         */
        PriorityQueue.prototype.peek = function () {
            return this.heap.peek();
        };
        /**
         * Returns true if this priority queue contains the specified element.
         * @param {Object} element element to search for.
         * @return {boolean} true if this priority queue contains the specified element,
         * false otherwise.
         */
        PriorityQueue.prototype.contains = function (element) {
            return this.heap.contains(element);
        };
        /**
         * Checks if this priority queue is empty.
         * @return {boolean} true if and only if this priority queue contains no items; false
         * otherwise.
         */
        PriorityQueue.prototype.isEmpty = function () {
            return this.heap.isEmpty();
        };
        /**
         * Returns the number of elements in this priority queue.
         * @return {number} the number of elements in this priority queue.
         */
        PriorityQueue.prototype.size = function () {
            return this.heap.size();
        };
        /**
         * Removes all of the elements from this priority queue.
         */
        PriorityQueue.prototype.clear = function () {
            this.heap.clear();
        };
        /**
         * Executes the provided function once for each element present in this queue in
         * no particular order.
         * @param {function(Object):*} callback function to execute, it is
         * invoked with one argument: the element value, to break the iteration you can
         * optionally return false.
         */
        PriorityQueue.prototype.forEach = function (callback) {
            this.heap.forEach(callback);
        };
        return PriorityQueue;
    })();
    collections.PriorityQueue = PriorityQueue; // end of priority queue
    var Set = (function () {
        /**
         * Creates an empty set.
         * @class <p>A set is a data structure that contains no duplicate items.</p>
         * <p>If the inserted elements are custom objects a function
         * which converts elements to strings must be provided. Example:</p>
         *
         * <pre>
         * function petToString(pet) {
         *  return pet.name;
         * }
         * </pre>
         *
         * @constructor
         * @param {function(Object):string=} toStringFunction optional function used
         * to convert elements to strings. If the elements aren't strings or if toString()
         * is not appropriate, a custom function which receives a onject and returns a
         * unique string must be provided.
         */
        function Set(toStringFunction) {
            this.dictionary = new Dictionary(toStringFunction);
        }
        /**
         * Returns true if this set contains the specified element.
         * @param {Object} element element to search for.
         * @return {boolean} true if this set contains the specified element,
         * false otherwise.
         */
        Set.prototype.contains = function (element) {
            return this.dictionary.containsKey(element);
        };
        /**
         * Adds the specified element to this set if it is not already present.
         * @param {Object} element the element to insert.
         * @return {boolean} true if this set did not already contain the specified element.
         */
        Set.prototype.add = function (element) {
            if (this.contains(element) || collections.isUndefined(element)) {
                return false;
            }
            else {
                this.dictionary.setValue(element, element);
                return true;
            }
        };
        /**
         * Performs an intersecion between this an another set.
         * Removes all values that are not present this set and the given set.
         * @param {collections.Set} otherSet other set.
         */
        Set.prototype.intersection = function (otherSet) {
            var set = this;
            this.forEach(function (element) {
                if (!otherSet.contains(element)) {
                    set.remove(element);
                }
                return true;
            });
        };
        /**
         * Performs a union between this an another set.
         * Adds all values from the given set to this set.
         * @param {collections.Set} otherSet other set.
         */
        Set.prototype.union = function (otherSet) {
            var set = this;
            otherSet.forEach(function (element) {
                set.add(element);
                return true;
            });
        };
        /**
         * Performs a difference between this an another set.
         * Removes from this set all the values that are present in the given set.
         * @param {collections.Set} otherSet other set.
         */
        Set.prototype.difference = function (otherSet) {
            var set = this;
            otherSet.forEach(function (element) {
                set.remove(element);
                return true;
            });
        };
        /**
         * Checks whether the given set contains all the elements in this set.
         * @param {collections.Set} otherSet other set.
         * @return {boolean} true if this set is a subset of the given set.
         */
        Set.prototype.isSubsetOf = function (otherSet) {
            if (this.size() > otherSet.size()) {
                return false;
            }
            var isSub = true;
            this.forEach(function (element) {
                if (!otherSet.contains(element)) {
                    isSub = false;
                    return false;
                }
                return true;
            });
            return isSub;
        };
        /**
         * Removes the specified element from this set if it is present.
         * @return {boolean} true if this set contained the specified element.
         */
        Set.prototype.remove = function (element) {
            if (!this.contains(element)) {
                return false;
            }
            else {
                this.dictionary.remove(element);
                return true;
            }
        };
        /**
         * Executes the provided function once for each element
         * present in this set.
         * @param {function(Object):*} callback function to execute, it is
         * invoked with one arguments: the element. To break the iteration you can
         * optionally return false.
         */
        Set.prototype.forEach = function (callback) {
            this.dictionary.forEach(function (k, v) {
                return callback(v);
            });
        };
        /**
         * Returns an array containing all of the elements in this set in arbitrary order.
         * @return {Array} an array containing all of the elements in this set.
         */
        Set.prototype.toArray = function () {
            return this.dictionary.values();
        };
        /**
         * Returns true if this set contains no elements.
         * @return {boolean} true if this set contains no elements.
         */
        Set.prototype.isEmpty = function () {
            return this.dictionary.isEmpty();
        };
        /**
         * Returns the number of elements in this set.
         * @return {number} the number of elements in this set.
         */
        Set.prototype.size = function () {
            return this.dictionary.size();
        };
        /**
         * Removes all of the elements from this set.
         */
        Set.prototype.clear = function () {
            this.dictionary.clear();
        };
        /*
        * Provides a string representation for display
        */
        Set.prototype.toString = function () {
            return collections.arrays.toString(this.toArray());
        };
        return Set;
    })();
    collections.Set = Set; // end of Set
    var Bag = (function () {
        /**
         * Creates an empty bag.
         * @class <p>A bag is a special kind of set in which members are
         * allowed to appear more than once.</p>
         * <p>If the inserted elements are custom objects a function
         * which converts elements to unique strings must be provided. Example:</p>
         *
         * <pre>
         * function petToString(pet) {
         *  return pet.name;
         * }
         * </pre>
         *
         * @constructor
         * @param {function(Object):string=} toStrFunction optional function used
         * to convert elements to strings. If the elements aren't strings or if toString()
         * is not appropriate, a custom function which receives an object and returns a
         * unique string must be provided.
         */
        function Bag(toStrFunction) {
            this.toStrF = toStrFunction || collections.defaultToString;
            this.dictionary = new Dictionary(this.toStrF);
            this.nElements = 0;
        }
        /**
        * Adds nCopies of the specified object to this bag.
        * @param {Object} element element to add.
        * @param {number=} nCopies the number of copies to add, if this argument is
        * undefined 1 copy is added.
        * @return {boolean} true unless element is undefined.
        */
        Bag.prototype.add = function (element, nCopies) {
            if (nCopies === void 0) { nCopies = 1; }
            if (collections.isUndefined(element) || nCopies <= 0) {
                return false;
            }
            if (!this.contains(element)) {
                var node = {
                    value: element,
                    copies: nCopies
                };
                this.dictionary.setValue(element, node);
            }
            else {
                this.dictionary.getValue(element).copies += nCopies;
            }
            this.nElements += nCopies;
            return true;
        };
        /**
        * Counts the number of copies of the specified object in this bag.
        * @param {Object} element the object to search for..
        * @return {number} the number of copies of the object, 0 if not found
        */
        Bag.prototype.count = function (element) {
            if (!this.contains(element)) {
                return 0;
            }
            else {
                return this.dictionary.getValue(element).copies;
            }
        };
        /**
         * Returns true if this bag contains the specified element.
         * @param {Object} element element to search for.
         * @return {boolean} true if this bag contains the specified element,
         * false otherwise.
         */
        Bag.prototype.contains = function (element) {
            return this.dictionary.containsKey(element);
        };
        /**
        * Removes nCopies of the specified object to this bag.
        * If the number of copies to remove is greater than the actual number
        * of copies in the Bag, all copies are removed.
        * @param {Object} element element to remove.
        * @param {number=} nCopies the number of copies to remove, if this argument is
        * undefined 1 copy is removed.
        * @return {boolean} true if at least 1 element was removed.
        */
        Bag.prototype.remove = function (element, nCopies) {
            if (nCopies === void 0) { nCopies = 1; }
            if (collections.isUndefined(element) || nCopies <= 0) {
                return false;
            }
            if (!this.contains(element)) {
                return false;
            }
            else {
                var node = this.dictionary.getValue(element);
                if (nCopies > node.copies) {
                    this.nElements -= node.copies;
                }
                else {
                    this.nElements -= nCopies;
                }
                node.copies -= nCopies;
                if (node.copies <= 0) {
                    this.dictionary.remove(element);
                }
                return true;
            }
        };
        /**
         * Returns an array containing all of the elements in this big in arbitrary order,
         * including multiple copies.
         * @return {Array} an array containing all of the elements in this bag.
         */
        Bag.prototype.toArray = function () {
            var a = [];
            var values = this.dictionary.values();
            var vl = values.length;
            for (var i = 0; i < vl; i++) {
                var node = values[i];
                var element = node.value;
                var copies = node.copies;
                for (var j = 0; j < copies; j++) {
                    a.push(element);
                }
            }
            return a;
        };
        /**
         * Returns a set of unique elements in this bag.
         * @return {collections.Set<T>} a set of unique elements in this bag.
         */
        Bag.prototype.toSet = function () {
            var toret = new Set(this.toStrF);
            var elements = this.dictionary.values();
            var l = elements.length;
            for (var i = 0; i < l; i++) {
                var value = elements[i].value;
                toret.add(value);
            }
            return toret;
        };
        /**
         * Executes the provided function once for each element
         * present in this bag, including multiple copies.
         * @param {function(Object):*} callback function to execute, it is
         * invoked with one argument: the element. To break the iteration you can
         * optionally return false.
         */
        Bag.prototype.forEach = function (callback) {
            this.dictionary.forEach(function (k, v) {
                var value = v.value;
                var copies = v.copies;
                for (var i = 0; i < copies; i++) {
                    if (callback(value) === false) {
                        return false;
                    }
                }
                return true;
            });
        };
        /**
         * Returns the number of elements in this bag.
         * @return {number} the number of elements in this bag.
         */
        Bag.prototype.size = function () {
            return this.nElements;
        };
        /**
         * Returns true if this bag contains no elements.
         * @return {boolean} true if this bag contains no elements.
         */
        Bag.prototype.isEmpty = function () {
            return this.nElements === 0;
        };
        /**
         * Removes all of the elements from this bag.
         */
        Bag.prototype.clear = function () {
            this.nElements = 0;
            this.dictionary.clear();
        };
        return Bag;
    })();
    collections.Bag = Bag; // End of bag 
    var BSTree = (function () {
        /**
         * Creates an empty binary search tree.
         * @class <p>A binary search tree is a binary tree in which each
         * internal node stores an element such that the elements stored in the
         * left subtree are less than it and the elements
         * stored in the right subtree are greater.</p>
         * <p>Formally, a binary search tree is a node-based binary tree data structure which
         * has the following properties:</p>
         * <ul>
         * <li>The left subtree of a node contains only nodes with elements less
         * than the node's element</li>
         * <li>The right subtree of a node contains only nodes with elements greater
         * than the node's element</li>
         * <li>Both the left and right subtrees must also be binary search trees.</li>
         * </ul>
         * <p>If the inserted elements are custom objects a compare function must
         * be provided at construction time, otherwise the <=, === and >= operators are
         * used to compare elements. Example:</p>
         * <pre>
         * function compare(a, b) {
         *  if (a is less than b by some ordering criterion) {
         *     return -1;
         *  } if (a is greater than b by the ordering criterion) {
         *     return 1;
         *  }
         *  // a must be equal to b
         *  return 0;
         * }
         * </pre>
         * @constructor
         * @param {function(Object,Object):number=} compareFunction optional
         * function used to compare two elements. Must return a negative integer,
         * zero, or a positive integer as the first argument is less than, equal to,
         * or greater than the second.
         */
        function BSTree(compareFunction) {
            this.root = null;
            this.compare = compareFunction || collections.defaultCompare;
            this.nElements = 0;
        }
        /**
         * Adds the specified element to this tree if it is not already present.
         * @param {Object} element the element to insert.
         * @return {boolean} true if this tree did not already contain the specified element.
         */
        BSTree.prototype.add = function (element) {
            if (collections.isUndefined(element)) {
                return false;
            }
            if (this.insertNode(this.createNode(element)) !== null) {
                this.nElements++;
                return true;
            }
            return false;
        };
        /**
         * Removes all of the elements from this tree.
         */
        BSTree.prototype.clear = function () {
            this.root = null;
            this.nElements = 0;
        };
        /**
         * Returns true if this tree contains no elements.
         * @return {boolean} true if this tree contains no elements.
         */
        BSTree.prototype.isEmpty = function () {
            return this.nElements === 0;
        };
        /**
         * Returns the number of elements in this tree.
         * @return {number} the number of elements in this tree.
         */
        BSTree.prototype.size = function () {
            return this.nElements;
        };
        /**
         * Returns true if this tree contains the specified element.
         * @param {Object} element element to search for.
         * @return {boolean} true if this tree contains the specified element,
         * false otherwise.
         */
        BSTree.prototype.contains = function (element) {
            if (collections.isUndefined(element)) {
                return false;
            }
            return this.searchNode(this.root, element) !== null;
        };
        /**
         * Removes the specified element from this tree if it is present.
         * @return {boolean} true if this tree contained the specified element.
         */
        BSTree.prototype.remove = function (element) {
            var node = this.searchNode(this.root, element);
            if (node === null) {
                return false;
            }
            this.removeNode(node);
            this.nElements--;
            return true;
        };
        /**
         * Executes the provided function once for each element present in this tree in
         * in-order.
         * @param {function(Object):*} callback function to execute, it is invoked with one
         * argument: the element value, to break the iteration you can optionally return false.
         */
        BSTree.prototype.inorderTraversal = function (callback) {
            this.inorderTraversalAux(this.root, callback, {
                stop: false
            });
        };
        /**
         * Executes the provided function once for each element present in this tree in pre-order.
         * @param {function(Object):*} callback function to execute, it is invoked with one
         * argument: the element value, to break the iteration you can optionally return false.
         */
        BSTree.prototype.preorderTraversal = function (callback) {
            this.preorderTraversalAux(this.root, callback, {
                stop: false
            });
        };
        /**
         * Executes the provided function once for each element present in this tree in post-order.
         * @param {function(Object):*} callback function to execute, it is invoked with one
         * argument: the element value, to break the iteration you can optionally return false.
         */
        BSTree.prototype.postorderTraversal = function (callback) {
            this.postorderTraversalAux(this.root, callback, {
                stop: false
            });
        };
        /**
         * Executes the provided function once for each element present in this tree in
         * level-order.
         * @param {function(Object):*} callback function to execute, it is invoked with one
         * argument: the element value, to break the iteration you can optionally return false.
         */
        BSTree.prototype.levelTraversal = function (callback) {
            this.levelTraversalAux(this.root, callback);
        };
        /**
         * Returns the minimum element of this tree.
         * @return {*} the minimum element of this tree or undefined if this tree is
         * is empty.
         */
        BSTree.prototype.minimum = function () {
            if (this.isEmpty()) {
                return undefined;
            }
            return this.minimumAux(this.root).element;
        };
        /**
         * Returns the maximum element of this tree.
         * @return {*} the maximum element of this tree or undefined if this tree is
         * is empty.
         */
        BSTree.prototype.maximum = function () {
            if (this.isEmpty()) {
                return undefined;
            }
            return this.maximumAux(this.root).element;
        };
        /**
         * Executes the provided function once for each element present in this tree in inorder.
         * Equivalent to inorderTraversal.
         * @param {function(Object):*} callback function to execute, it is
         * invoked with one argument: the element value, to break the iteration you can
         * optionally return false.
         */
        BSTree.prototype.forEach = function (callback) {
            this.inorderTraversal(callback);
        };
        /**
         * Returns an array containing all of the elements in this tree in in-order.
         * @return {Array} an array containing all of the elements in this tree in in-order.
         */
        BSTree.prototype.toArray = function () {
            var array = [];
            this.inorderTraversal(function (element) {
                array.push(element);
                return true;
            });
            return array;
        };
        /**
         * Returns the height of this tree.
         * @return {number} the height of this tree or -1 if is empty.
         */
        BSTree.prototype.height = function () {
            return this.heightAux(this.root);
        };
        /**
        * @private
        */
        BSTree.prototype.searchNode = function (node, element) {
            var cmp = null;
            while (node !== null && cmp !== 0) {
                cmp = this.compare(element, node.element);
                if (cmp < 0) {
                    node = node.leftCh;
                }
                else if (cmp > 0) {
                    node = node.rightCh;
                }
            }
            return node;
        };
        /**
        * @private
        */
        BSTree.prototype.transplant = function (n1, n2) {
            if (n1.parent === null) {
                this.root = n2;
            }
            else if (n1 === n1.parent.leftCh) {
                n1.parent.leftCh = n2;
            }
            else {
                n1.parent.rightCh = n2;
            }
            if (n2 !== null) {
                n2.parent = n1.parent;
            }
        };
        /**
        * @private
        */
        BSTree.prototype.removeNode = function (node) {
            if (node.leftCh === null) {
                this.transplant(node, node.rightCh);
            }
            else if (node.rightCh === null) {
                this.transplant(node, node.leftCh);
            }
            else {
                var y = this.minimumAux(node.rightCh);
                if (y.parent !== node) {
                    this.transplant(y, y.rightCh);
                    y.rightCh = node.rightCh;
                    y.rightCh.parent = y;
                }
                this.transplant(node, y);
                y.leftCh = node.leftCh;
                y.leftCh.parent = y;
            }
        };
        /**
        * @private
        */
        BSTree.prototype.inorderTraversalAux = function (node, callback, signal) {
            if (node === null || signal.stop) {
                return;
            }
            this.inorderTraversalAux(node.leftCh, callback, signal);
            if (signal.stop) {
                return;
            }
            signal.stop = callback(node.element) === false;
            if (signal.stop) {
                return;
            }
            this.inorderTraversalAux(node.rightCh, callback, signal);
        };
        /**
        * @private
        */
        BSTree.prototype.levelTraversalAux = function (node, callback) {
            var queue = new Queue();
            if (node !== null) {
                queue.enqueue(node);
            }
            while (!queue.isEmpty()) {
                node = queue.dequeue();
                if (callback(node.element) === false) {
                    return;
                }
                if (node.leftCh !== null) {
                    queue.enqueue(node.leftCh);
                }
                if (node.rightCh !== null) {
                    queue.enqueue(node.rightCh);
                }
            }
        };
        /**
        * @private
        */
        BSTree.prototype.preorderTraversalAux = function (node, callback, signal) {
            if (node === null || signal.stop) {
                return;
            }
            signal.stop = callback(node.element) === false;
            if (signal.stop) {
                return;
            }
            this.preorderTraversalAux(node.leftCh, callback, signal);
            if (signal.stop) {
                return;
            }
            this.preorderTraversalAux(node.rightCh, callback, signal);
        };
        /**
        * @private
        */
        BSTree.prototype.postorderTraversalAux = function (node, callback, signal) {
            if (node === null || signal.stop) {
                return;
            }
            this.postorderTraversalAux(node.leftCh, callback, signal);
            if (signal.stop) {
                return;
            }
            this.postorderTraversalAux(node.rightCh, callback, signal);
            if (signal.stop) {
                return;
            }
            signal.stop = callback(node.element) === false;
        };
        /**
        * @private
        */
        BSTree.prototype.minimumAux = function (node) {
            while (node.leftCh !== null) {
                node = node.leftCh;
            }
            return node;
        };
        /**
        * @private
        */
        BSTree.prototype.maximumAux = function (node) {
            while (node.rightCh !== null) {
                node = node.rightCh;
            }
            return node;
        };
        /**
          * @private
          */
        BSTree.prototype.heightAux = function (node) {
            if (node === null) {
                return -1;
            }
            return Math.max(this.heightAux(node.leftCh), this.heightAux(node.rightCh)) + 1;
        };
        /*
        * @private
        */
        BSTree.prototype.insertNode = function (node) {
            var parent = null;
            var position = this.root;
            var cmp = null;
            while (position !== null) {
                cmp = this.compare(node.element, position.element);
                if (cmp === 0) {
                    return null;
                }
                else if (cmp < 0) {
                    parent = position;
                    position = position.leftCh;
                }
                else {
                    parent = position;
                    position = position.rightCh;
                }
            }
            node.parent = parent;
            if (parent === null) {
                // tree is empty
                this.root = node;
            }
            else if (this.compare(node.element, parent.element) < 0) {
                parent.leftCh = node;
            }
            else {
                parent.rightCh = node;
            }
            return node;
        };
        /**
        * @private
        */
        BSTree.prototype.createNode = function (element) {
            return {
                element: element,
                leftCh: null,
                rightCh: null,
                parent: null
            };
        };
        return BSTree;
    })();
    collections.BSTree = BSTree; // end of BSTree
})(collections || (collections = {})); // End of module 
var BrushStroke = (function () {
    function BrushStroke(brush, stroke) {
        this.brush = brush;
        this.stroke = stroke;
    }
    return BrushStroke;
})();
/// <reference path="brush/BrushStroke.ts"/>
var InkCanvas = (function () {
    function InkCanvas(canvas) {
        this._canvas = canvas;
        this._context = canvas.getContext("2d");
        this._isDrawing = false;
        this._brushStrokes = [];
        this._brush = null;
        this._scrollOffset = { x: 0, y: 0 };
    }
    InkCanvas.prototype.drawStroke = function (stroke, brush) {
        if (brush)
            this._brush = brush;
        this._scrollOffset = { x: 0, y: 0 };
        this._isDrawing = true;
        this._activeStroke = new BrushStroke(this._brush, new Stroke());
        this._activeStroke.stroke.documentOffsetY = window.pageYOffset;
        var first = stroke.points[0];
        var last = stroke.points[stroke.points.length - 1];
        this.startDrawing(first.x, first.y, brush);
        for (var i = 1; i < stroke.points.length - 2; i++) {
            this.draw(stroke.points[i].x, stroke.points[i].y);
        }
        this.endDrawing(last.x, last.y);
    };
    InkCanvas.prototype.startDrawing = function (x, y, brush) {
        if (brush)
            this._brush = brush;
        this._brush.init(x, y, this);
        this._scrollOffset = { x: 0, y: 0 };
        this._isDrawing = true;
        this._activeStroke = new BrushStroke(this._brush, new Stroke());
        this._activeStroke.stroke.documentOffsetY = window.pageYOffset;
        this.draw(x, y);
    };
    InkCanvas.prototype.draw = function (x, y) {
        if (this._isDrawing == false)
            return;
        this._activeStroke.stroke.points.push({ x: x, y: y });
        this._brush.draw(x, y, this);
    };
    InkCanvas.prototype.endDrawing = function (x, y) {
        this.draw(x, y);
        this._isDrawing = false;
        this._brushStrokes.push(this._activeStroke);
    };
    InkCanvas.prototype.addBrushStroke = function (brushStroke) {
        if (this._brushStrokes.indexOf(brushStroke) == -1)
            this._brushStrokes.push(brushStroke);
    };
    InkCanvas.prototype.removeBrushStroke = function (brushStroke) {
        var index = this._brushStrokes.indexOf(brushStroke);
        if (index > -1) {
            this._brushStrokes.splice(index, 1);
            return true;
        }
        return false;
        console.log("couldn't remove element");
    };
    InkCanvas.prototype.update = function () {
        this._scrollOffset = { x: window.pageXOffset, y: window.pageYOffset };
        this._context.clearRect(0, 0, this._canvas.width, this._canvas.height);
        for (var i = 0; i < this._brushStrokes.length; i++) {
            this._brushStrokes[i]["brush"].drawStroke(this._brushStrokes[i]["stroke"], this);
        }
    };
    InkCanvas.prototype.setBrush = function (brush) {
        this._brush = brush;
        if (this._isDrawing) {
            this._activeStroke.brush = brush;
            var p = this._activeStroke.stroke.points[0];
            this._brush.init(p.x, p.y, this);
        }
    };
    InkCanvas.prototype.redrawActiveStroke = function () {
        this.update();
        this._activeStroke.brush.drawStroke(this._activeStroke.stroke, this);
    };
    ///called after lineSelection so that highlights for line selection disappear
    ///bracket selections are yet updated
    InkCanvas.prototype.removeStroke = function () {
        this._context.clearRect(0, 0, this._canvas.width, this._canvas.height);
        this.update();
    };
    InkCanvas.prototype.hide = function () {
        console.log("hide canvas");
    };
    InkCanvas.prototype.reveal = function () {
        console.log("reveal canvas");
    };
    return InkCanvas;
})();
var StrokeType;
(function (StrokeType) {
    StrokeType[StrokeType["Null"] = 0] = "Null";
    StrokeType[StrokeType["Line"] = 1] = "Line";
    StrokeType[StrokeType["Bracket"] = 2] = "Bracket";
    StrokeType[StrokeType["Marquee"] = 3] = "Marquee";
    StrokeType[StrokeType["Scribble"] = 4] = "Scribble";
    StrokeType[StrokeType["MultiLine"] = 5] = "MultiLine";
})(StrokeType || (StrokeType = {}));
var HighlightBrush = (function () {
    function HighlightBrush() {
        this._img = new Image();
        this._img.src = chrome.extension.getURL("assets/brush.png");
    }
    HighlightBrush.prototype.init = function (x, y, inkCanvas) {
        // do nothing
    };
    HighlightBrush.prototype.draw = function (x, y, inkCanvas) {
        inkCanvas._context.globalCompositeOperation = "xor";
        inkCanvas._context.globalAlpha = 0.6;
        inkCanvas._context.drawImage(this._img, x - 15, y - 15, 30, 30);
    };
    HighlightBrush.prototype.drawStroke = function (stroke, inkCanvas) {
        for (var i = 0; i < stroke.points.length; i++) {
            var p = stroke.points[i];
            inkCanvas._context.globalCompositeOperation = "xor";
            inkCanvas._context.globalAlpha = 0.6;
            inkCanvas._context.drawImage(this._img, p.x - inkCanvas._scrollOffset.x + stroke.documentOffsetX - 15, p.y + stroke.documentOffsetY - inkCanvas._scrollOffset.y - 15, 30, 30);
        }
    };
    return HighlightBrush;
})();
var SelectionBrush = (function () {
    function SelectionBrush(rect) {
        this._rect = rect;
    }
    SelectionBrush.prototype.init = function (x, y, inkCanvas) {
        // do nothing
    };
    SelectionBrush.prototype.draw = function (x, y, inkCanvas) {
        // do nothing.
    };
    SelectionBrush.prototype.drawStroke = function (stroke, inkCanvas) {
        if (this._rect != null) {
            stroke = new Stroke();
            stroke.points.push({ x: this._rect.x, y: this._rect.y });
            stroke.points.push({ x: this._rect.x + this._rect.w, y: this._rect.y + this._rect.h });
        }
        var startX = stroke.points[0].x;
        var startY = stroke.points[0].y;
        var w = stroke.points[stroke.points.length - 1].x - startX;
        var h = stroke.points[stroke.points.length - 1].y - startY;
        startX = startX - inkCanvas._scrollOffset.x + stroke.documentOffsetX;
        startY = startY - inkCanvas._scrollOffset.y + stroke.documentOffsetY;
        var ctx = inkCanvas._context;
        ctx.globalCompositeOperation = "source-over";
        ctx.globalAlpha = 0.6;
        ctx.beginPath();
        ctx.fillStyle = "rgb(222,214,0)";
        ctx.fillRect(startX, startY, w, h);
        ctx.fill();
    };
    return SelectionBrush;
})();
var Rectangle = (function () {
    function Rectangle(x, y, w, h) {
        this.x = x;
        this.y = y;
        this.w = w;
        this.h = h;
    }
    Rectangle.prototype.intersectsRectangle = function (r2) {
        return !(r2.x > this.x + this.w ||
            r2.x + r2.w < this.x ||
            r2.y > this.y + this.h ||
            r2.y + r2.h < this.y);
    };
    return Rectangle;
})();
/// <reference path="../../typings/jquery/jquery.d.ts"/>
var DomUtil = (function () {
    function DomUtil() {
    }
    DomUtil.getCommonAncestor = function (a, b) {
        var parentsa = $(a).parents().toArray();
        var parentsb = $(b).parents().toArray();
        parentsa.unshift(a);
        parentsb.unshift(b);
        var found = null;
        $.each(parentsa, function () {
            var thisa = this;
            $.each(parentsb, function () {
                if (thisa == this) {
                    found = this;
                    return false;
                }
            });
            if (found)
                return false;
        });
        return found;
    };
    return DomUtil;
})();
var AbstractSelection = (function () {
    function AbstractSelection(className) {
        this.className = className;
    }
    AbstractSelection.prototype.start = function (x, y) { };
    AbstractSelection.prototype.update = function (x, y) { };
    AbstractSelection.prototype.end = function (x, y) { };
    AbstractSelection.prototype.deselect = function () { };
    AbstractSelection.prototype.getBoundingRect = function () { return null; };
    AbstractSelection.prototype.analyzeContent = function () { };
    AbstractSelection.prototype.getContent = function () { return null; };
    return AbstractSelection;
})();
/// <reference path="../../typings/jquery/jquery.d.ts"/>
/// <reference path="../ink/InkCanvas.ts"/>
/// <reference path="../ink/brush/BrushStroke.ts"/>
/// <reference path="../ink/brush/HighlightBrush.ts"/>
/// <reference path="../ink/brush/SelectionBrush.ts"/>
/// <reference path="../util/Rectangle.ts"/>
/// <reference path="../util/DomUtil.ts"/>
/// <reference path="AbstractSelection.ts"/>
var LineSelection = (function (_super) {
    __extends(LineSelection, _super);
    function LineSelection(inkCanvas) {
        _super.call(this, "LineSelection");
        this._brushStroke = null;
        this._inkCanvas = inkCanvas;
    }
    LineSelection.prototype.start = function (x, y) {
        this._inkCanvas.startDrawing(x, y, new HighlightBrush());
    };
    LineSelection.prototype.update = function (x, y) {
        this._inkCanvas.draw(x, y);
    };
    LineSelection.prototype.end = function (x, y) {
        this._inkCanvas.endDrawing(x, y);
        this._brushStroke = this._inkCanvas._activeStroke;
        this.analyzeContent();
        this._brushStroke.brush = new SelectionBrush(this.getBoundingRect());
        this._inkCanvas.update();
    };
    LineSelection.prototype.deselect = function () {
        this._inkCanvas.removeBrushStroke(this._brushStroke);
    };
    LineSelection.prototype.getBoundingRect = function () {
        var minX = 1000000;
        var maxX = -1000000;
        var minY = 1000000;
        var maxY = -1000000;
        for (var i = 0; i < this._clientRects.length; i++) {
            var p = this._clientRects[i];
            maxY = p.top + p.height > maxY ? p.top + p.height : maxY;
            maxX = p.left + p.width > maxX ? p.left + p.width : maxX;
            minX = p.left < minX ? p.left : minX;
            minY = p.top < minY ? p.top : minY;
        }
        return new Rectangle(minX, minY + window.pageYOffset, maxX - minX, maxY - minY);
    };
    LineSelection.prototype.addWordTag = function (nodes) {
        var _this = this;
        $.each(nodes, function (index, value) {
            if (value.nodeType == Node.TEXT_NODE) {
                $(value).replaceWith($(value).text().replace(/([^,\s]*)/g, "<word>$1</word>"));
            }
            else if (value.childNodes.length > 0) {
                _this.addWordTag(value.childNodes);
            }
        });
    };
    LineSelection.prototype.analyzeContent = function () {
        console.log("analyzing content.");
        var stroke = this._brushStroke.stroke;
        var pStart = stroke.points[0];
        var pEnd = stroke.points[stroke.points.length - 1];
        var nStart = document.elementFromPoint(pStart.x, pStart.y);
        var nEnd = document.elementFromPoint(pEnd.x, pEnd.y);
        var commonParent = DomUtil.getCommonAncestor(nStart, nEnd);
        console.log(commonParent);
        var nodes = $(commonParent).contents();
        if (nodes.length > 0) {
            var original_content = $(commonParent).clone();
            $.each(nodes, function () {
                if (this.nodeType == Node.TEXT_NODE) {
                    $(this).replaceWith($(this).text().replace(/([^,\s]*)/g, "<word>$1</word>"));
                }
            });
            nStart = document.elementFromPoint(pStart.x, pStart.y);
            nEnd = document.elementFromPoint(pEnd.x, pEnd.y);
            this._range = new Range();
            this._range.setStart(nStart, 0);
            this._range.setEndAfter(nEnd);
            this._clientRects = this._range.getClientRects();
            var frag = this._range.cloneContents();
            var result = "";
            $.each(frag["children"], function () {
                result += $(this)[0].outerHTML.replace(/<word>|<\/word>/g, " ");
            });
            result = result.replace(/\s\s+/g, ' ').trim();
            this._content = result;
            $(commonParent).replaceWith(original_content);
        }
    };
    LineSelection.prototype.getContent = function () {
        return this._content;
    };
    return LineSelection;
})(AbstractSelection);
var LineBrush = (function () {
    function LineBrush() {
    }
    LineBrush.prototype.init = function (x, y, inkCanvas) {
        inkCanvas._context.beginPath();
        inkCanvas._context.globalAlpha = 1;
        inkCanvas._context.globalCompositeOperation = "source-over";
        inkCanvas._context.strokeStyle = "rgb(192,0,0)";
        inkCanvas._context.lineWidth = 6;
        inkCanvas._context.lineJoin = inkCanvas._context.lineCap = 'round';
        inkCanvas._context.moveTo(x, y);
    };
    LineBrush.prototype.draw = function (x, y, inkCanvas) {
        inkCanvas._context.globalAlpha = 1;
        inkCanvas._context.globalCompositeOperation = "source-over";
        inkCanvas._context.strokeStyle = "rgb(192,0,0)";
        inkCanvas._context.lineWidth = 6;
        inkCanvas._context.lineJoin = inkCanvas._context.lineCap = 'round';
        inkCanvas._context.lineTo(x, y);
        inkCanvas._context.stroke();
        inkCanvas._context.moveTo(x, y);
    };
    LineBrush.prototype.drawStroke = function (stroke, inkCanvas) {
        var first = stroke.points[0];
        inkCanvas._context.beginPath();
        inkCanvas._context.strokeStyle = "rgb(192,0,0)";
        inkCanvas._context.lineWidth = 6;
        inkCanvas._context.lineJoin = inkCanvas._context.lineCap = 'round';
        inkCanvas._context.moveTo(first.x, first.y);
        for (var i = 1; i < stroke.points.length; i++) {
            var p = stroke.points[i];
            inkCanvas._context.lineTo(p.x, p.y);
            inkCanvas._context.stroke();
            inkCanvas._context.moveTo(p.x, p.y);
        }
    };
    return LineBrush;
})();
/// <reference path="../../typings/jquery/jquery.d.ts"/>
/// <reference path="../ink/InkCanvas.ts"/>
/// <reference path="../ink/brush/BrushStroke.ts"/>
/// <reference path="../ink/brush/HighlightBrush.ts"/>
/// <reference path="../ink/brush/SelectionBrush.ts"/>
/// <reference path="../ink/brush/LineBrush.ts"/>
/// <reference path="../util/Rectangle.ts"/>
/// <reference path="../util/DomUtil.ts"/>
var UnknownSelection = (function (_super) {
    __extends(UnknownSelection, _super);
    function UnknownSelection(inkCanvas, fromActiveStroke) {
        if (fromActiveStroke === void 0) { fromActiveStroke = false; }
        _super.call(this, "UnknownSelection");
        this._brushStroke = null;
        this._inkCanvas = inkCanvas;
        if (fromActiveStroke) {
            inkCanvas.setBrush(new LineBrush());
        }
    }
    UnknownSelection.prototype.start = function (x, y) {
        this._inkCanvas.startDrawing(x, y, new LineBrush());
    };
    UnknownSelection.prototype.update = function (x, y) {
        this._inkCanvas.draw(x, y);
    };
    UnknownSelection.prototype.end = function (x, y) {
        this._inkCanvas.endDrawing(x, y);
    };
    UnknownSelection.prototype.deselect = function () {
        this._inkCanvas.removeBrushStroke(this._brushStroke);
    };
    UnknownSelection.prototype.getBoundingRect = function () {
        return this._brushStroke.stroke.getBoundingRect();
    };
    UnknownSelection.prototype.analyzeContent = function () {
        // nothing to analyze.
    };
    UnknownSelection.prototype.getContent = function () {
        return null;
    };
    return UnknownSelection;
})(AbstractSelection);
/// <reference path="../typings/chrome/chrome.d.ts"/>
/// <reference path="../typings/jquery/jquery.d.ts"/>
/// <reference path="ink/InkCanvas.ts"/>
/// <reference path="ink/StrokeType.ts" />
/// <reference path="selection/LineSelection.ts"/>
/// <reference path="selection/UnknownSelection.ts"/>
var Main = (function () {
    function Main() {
        var _this = this;
        this.prevStrokeType = StrokeType.Line;
        this.currentStrokeType = StrokeType.Line;
        this.selections = new Array();
        this.urlGroup = Date.now();
        this.previousSelections = new Array();
        this.mouseMove = function (e) {
            if (!_this.isSelecting) {
                return;
            }
            //if (this.currentStrokeType == StrokeType.MultiLine)
            //    document.body.removeChild(this.canvas);
            if (_this.currentStrokeType == StrokeType.Bracket || _this.currentStrokeType == StrokeType.Marquee) {
                var currType = GestireClassifier.getGestureType(_this.inkCanvas._activeStroke.stroke);
                if (_this.currentStrokeType == StrokeType.Bracket && currType == GestureType.Diagonal) {
                    _this.selection = new MarqueeSelection(_this.inkCanvas, true);
                    _this.currentStrokeType = StrokeType.Marquee;
                    _this.inkCanvas.redrawActiveStroke();
                }
                if (_this.currentStrokeType == StrokeType.Marquee && currType == GestureType.Horizontal) {
                    _this.selection = new BracketSelection(_this.inkCanvas, true);
                    _this.currentStrokeType = StrokeType.Bracket;
                    _this.inkCanvas.redrawActiveStroke();
                }
            }
            _this.selection.update(e.clientX, e.clientY);
            document.body.appendChild(_this.canvas);
        };
        this.documentDown = function (e) {
            switch (_this.currentStrokeType) {
                case StrokeType.MultiLine:
                    _this.selection = new MultiLineSelection(_this.inkCanvas);
                    break;
                case StrokeType.Bracket:
                    _this.selection = new BracketSelection(_this.inkCanvas);
                    break;
                case StrokeType.Marquee:
                    _this.selection = new MarqueeSelection(_this.inkCanvas);
                    break;
            }
            console.log("current selection:" + _this.currentStrokeType);
            _this.selection.start(e.clientX, e.clientY);
            document.body.appendChild(_this.canvas);
            _this.canvas.addEventListener("mousemove", _this.mouseMove);
            _this.isSelecting = true;
        };
        this.documentScroll = function (e) {
            _this.inkCanvas.update();
        };
        this.windowUp = function (e) {
            if (!_this.isSelecting)
                return;
            _this.canvas.removeEventListener("mousemove", _this.mouseMove);
            _this.inkCanvas.removeBrushStroke(_this.inkCanvas._activeStroke);
            _this.inkCanvas.update();
            _this.isSelecting = false;
        };
        this.canvasUp = function (e) {
            if (!_this.isSelecting) {
                return;
            }
            _this.canvas.removeEventListener("mousemove", _this.mouseMove);
            document.body.removeChild(_this.canvas);
            $(_this.menuIframe).hide();
            _this.selection.end(e.clientX, e.clientY);
            var stroke = _this.inkCanvas._activeStroke.stroke.getCopy();
            var currType = GestireClassifier.getGestureType(stroke);
            if (currType == GestureType.Null) {
                console.log("JUST A TAP");
                document.body.appendChild(_this.canvas);
                _this.inkCanvas.update();
                return;
            }
            else if (currType == GestureType.Scribble) {
                var segments = stroke.breakUp();
                var p0 = stroke.points[0];
                var p1 = stroke.points[stroke.points.length - 1];
                var line = Line.fromPoint(p0, p1);
                var intersectionCount = 0;
                $.each(segments, function () {
                    var intersects = line.intersectsLine(this);
                    if (intersects)
                        intersectionCount++;
                });
                if (intersectionCount > 2) {
                    var strokeBB = stroke.getBoundingRect();
                    strokeBB.y += stroke.documentOffsetY;
                    _this.selections.forEach(function (s) {
                        try {
                            if (s.getBoundingRect().intersectsRectangle(strokeBB)) {
                                s.deselect();
                                var selectionIndex = _this.selections.indexOf(s);
                                if (selectionIndex > -1) {
                                    _this.selections.splice(selectionIndex, 1);
                                }
                            }
                        }
                        catch (e) {
                            console.log(e);
                            console.log(_this);
                        }
                    });
                }
                _this.inkCanvas.removeBrushStroke(_this.inkCanvas._activeStroke);
            }
            else {
                _this.selection.id = Date.now();
                _this.selection.url = window.location.protocol + "//" + window.location.host + window.location.pathname;
                _this.selections.push(_this.selection);
                /*
                var selectionInfo = {};
                selectionInfo["id"] = this.selection["id"];
                selectionInfo["url"] = window.location.protocol + "//" + window.location.host + window.location.pathname;
                selectionInfo["boundingRect"] = this.selection.getBoundingRect();
                selectionInfo["date"] = (new Date()).toString();
                selectionInfo["title"] = document.title;
                selectionInfo["content"] = this.relativeToAbsolute(this.selection.getContent());
                */
                chrome.runtime.sendMessage({ msg: "store_selection", data: _this.selection });
            }
            if (_this.currentStrokeType == StrokeType.Bracket || _this.currentStrokeType == StrokeType.Marquee) {
                _this.currentStrokeType = StrokeType.Bracket;
            }
            document.body.appendChild(_this.canvas);
            $(_this.menuIframe).show();
            _this.inkCanvas.update();
            _this.isSelecting = false;
            _this.updateSelectedList();
        };
        console.log("Starting NuSys.");
        // create and append canvas
        var body = document.body, html = document.documentElement;
        Main.DOC_WIDTH = Math.max(body.scrollWidth, body.offsetWidth, html.clientWidth, html.scrollWidth, html.offsetWidth);
        Main.DOC_HEIGHT = Math.max(body.scrollHeight, body.offsetHeight, html.clientHeight, html.scrollHeight, html.offsetHeight);
        this.canvas = document.createElement("canvas");
        this.canvas.width = window.innerWidth;
        this.canvas.height = window.innerHeight;
        this.canvas.style.position = "fixed";
        this.canvas.style.top = "0";
        this.canvas.style.left = "0"; //fixes canvas placements
        this.canvas.style.zIndex = "998";
        this.inkCanvas = new InkCanvas(this.canvas);
        chrome.storage.local.get(null, function (data) {
            _this.previousSelections = data["selections"];
        });
        chrome.runtime.onMessage.addListener(function (request, sender, sendResponse) {
            console.log(request);
            if (request.msg == "check_injection")
                sendResponse(true);
            if (request.msg == "init") {
                _this.init(request.data);
                sendResponse();
            }
            if (request.msg == "show_menu") {
                _this.showMenu();
            }
            if (request.msg == "hide_menu") {
                _this.hideMenu();
            }
            if (request.msg == "enable_selections") {
                _this.toggleEnabled(true);
            }
            if (request.msg == "disable_selections") {
                _this.toggleEnabled(false);
            }
            if (request.msg == "set_selections") {
                _this.selections = [];
                request.data.forEach(function (d) {
                    var ls = null;
                    console.log("inkcanvas");
                    console.log(_this.inkCanvas);
                    if (d["className"] == "LineSelection")
                        ls = new LineSelection(_this.inkCanvas);
                    if (d["className"] == "BracketSelection")
                        ls = new BracketSelection(_this.inkCanvas);
                    if (d["className"] == "MarqueeSelection")
                        ls = new MarqueeSelection(_this.inkCanvas);
                    if (d["className"] == "MultiLineSelection")
                        ls = new MultiLineSelection(_this.inkCanvas);
                    $.extend(ls, d);
                    ls._inkCanvas = _this.inkCanvas;
                    var stroke = new Stroke();
                    console.log(ls);
                    $.extend(stroke, ls._brushStroke.stroke);
                    ls._brushStroke.stroke = stroke;
                    console.log("marked content");
                    console.log(ls.getContent());
                    /*
                    document.body.removeChild(this.canvas);
                    ls.analyzeContent();
                    this.inkCanvas.addBrushStroke(new BrushStroke(new SelectionBrush(ls.getBoundingRect()), new Stroke()));
                    this.inkCanvas.update();
                    document.body.appendChild(this.canvas);
                    */
                });
            }
        });
    }
    Main.prototype.init = function (menuHtml) {
        var _this = this;
        this.menuIframe = $("<iframe frameborder=0>")[0];
        document.body.appendChild(this.menuIframe);
        this.menu = $(menuHtml)[0];
        $(this.menuIframe).css({ position: "fixed", top: "1px", right: "1px", width: "410px", height: "90px", "z-index": 1001 });
        $(this.menuIframe).contents().find('html').html(this.menu.outerHTML);
        $(this.menuIframe).css("display", "none");
        $(this.menuIframe).contents().find("#btnLineSelect").click(function () {
            console.log("switching to multiline selection");
            _this.currentStrokeType = StrokeType.MultiLine;
        });
        $(this.menuIframe).contents().find("#btnBlockSelect").click(function () {
            _this.currentStrokeType = StrokeType.Bracket;
        });
        $(this.menuIframe).contents().find("#btnClear").click(function () {
            chrome.runtime.sendMessage({ msg: "clear_page_selections" });
        });
        chrome.runtime.sendMessage({ msg: "query_active" }, function (isActive) {
            $(_this.menuIframe).contents().find("#toggle").prop("checked", isActive);
        });
    };
    Main.prototype.showMenu = function () {
        var _this = this;
        this.isMenuVisible = true;
        $(this.menuIframe).css("display", "block");
        $(this.menuIframe).contents().find("#toggle").change(function () {
            chrome.runtime.sendMessage({ msg: "set_active", data: $(_this.menuIframe).contents().find("#toggle").prop("checked") });
        });
        $(this.menuIframe).contents().find("#btnExpand").click(function () {
            console.log("expanding.");
            var list = $(_this.menuIframe).contents().find("#selected_list");
            if (list.css("display") == "none") {
                list.css("display", "block");
                $(_this.menuIframe).css("height", "500px");
            }
            else {
                list.css("display", "none");
                $(_this.menuIframe).css("height", "80px");
            }
        });
    };
    Main.prototype.hideMenu = function () {
        this.isMenuVisible = false;
        $(this.menuIframe).css("display", "none");
    };
    Main.prototype.toggleEnabled = function (flag) {
        $(this.menuIframe).contents().find("#toggle").prop("checked", flag);
        //called to add or remove canvas when toggle has been changed
        this.isEnabled = flag;
        if (this.isEnabled) {
            window.addEventListener("mouseup", this.windowUp);
            document.body.addEventListener("mousedown", this.documentDown);
            document.addEventListener("scroll", this.documentScroll);
            this.canvas.addEventListener("mouseup", this.canvasUp);
            document.body.appendChild(this.canvas);
            this.inkCanvas.update();
        }
        else {
            window.removeEventListener("mouseup", this.windowUp);
            document.body.removeEventListener("mousedown", this.documentDown);
            document.removeEventListener("scroll", this.documentScroll);
            this.canvas.removeEventListener("mouseup", this.canvasUp);
            try {
                document.body.removeChild(this.canvas);
            }
            catch (e) {
                console.log("no canvas visible." + e);
            }
        }
    };
    Main.prototype.updateSelectedList = function () {
        var list = $(this.menuIframe).contents().find("#selected_list");
        list.empty();
        this.selections.forEach(function (s) {
            list.append("<div class='selected_list_item'>" + s.getContent() + "</div>");
        });
    };
    Main.prototype.relativeToAbsolute = function (content) {
        //////change relative href of hyperlink and src of image in html string to absolute
        chrome.storage.local.get(null, function (data) { console.info(data); });
        var res = content.split('href="');
        var newval = res[0];
        for (var i = 1; i < res.length; i++) {
            newval += 'href="';
            if (res[i].slice(0, 4) != "http") {
                newval += window.location.protocol + "//" + window.location.host;
            }
            newval += res[i];
        }
        var src = newval.split('src="');
        var finalval = src[0];
        for (var i = 1; i < src.length; i++) {
            finalval += 'src="';
            if (src[i].slice(0, 4) != "http" && src[i].slice(0, 2) != "//") {
                finalval += window.location["origin"];
                var path = window.location.pathname;
                var pathSplit = path.split('/');
                var newpath = "";
                var pIndex = pathSplit.length - 1;
                $(pathSplit).each(function (indx, elem) {
                    if (indx < pathSplit.length - 1) {
                        newpath += (elem + "/");
                    }
                });
                var newpathSplit = newpath.split("/");
                var p = "";
                pIndex = newpathSplit.length - 1;
                if (src[i][0] == "/") {
                    pIndex = pIndex - 1;
                }
                else {
                    src[i] = "/" + src[i];
                }
                $(newpathSplit).each(function (index, elem) {
                    if (index < pIndex) {
                        p += (elem + "/");
                    }
                });
                p = p.substring(0, p.length - 1);
                newpath = p;
                finalval += newpath;
            }
            finalval += src[i];
        }
        return finalval;
    };
    Main.prototype.drawAllSelections = function (prevSelections) {
        var _this = this;
        this.selections.forEach(function (selection) {
            var rect = selection["boundingRect"];
            var stroke = new Stroke();
            stroke.points.push({ x: rect.x, y: rect.y });
            stroke.points.push({ x: rect.x + rect.w, y: rect.y + rect.h });
            _this.inkCanvas.drawStroke(stroke, new SelectionBrush(rect));
        });
        this.inkCanvas.update();
    };
    Main.prototype.drawPastSelections = function (rectArray) {
        var _this = this;
        $.each(rectArray, function (index, rect) {
            var stroke = new Stroke();
            stroke.points.push({ x: rect.x, y: rect.y });
            stroke.points.push({ x: rect.x + rect.w, y: rect.y + rect.h });
            _this.inkCanvas.drawStroke(stroke, new SelectionBrush(rect));
        });
        this.inkCanvas.update();
    };
    return Main;
})();
/// <reference path="Main.ts"/>
var greeter = new Main();
var MarqueeBrush = (function () {
    function MarqueeBrush(x, y) {
        this._startX = x;
        this._startY = y;
    }
    MarqueeBrush.prototype.init = function (x, y, inkCanvas) {
        inkCanvas._context.lineWidth = 4;
        inkCanvas._context.lineJoin = inkCanvas._context.lineCap = 'butt';
    };
    MarqueeBrush.prototype.draw = function (x, y, inkCanvas) {
        var canvas = inkCanvas._canvas;
        var ctx = inkCanvas._context;
        ctx.globalCompositeOperation = "source-over";
        ctx.beginPath();
        ctx.lineWidth = 5;
        ctx.strokeStyle = "rgb(222,214,0)";
        ctx.setLineDash([6]);
        ctx.rect(this._startX, this._startY, x - this._startX, y - this._startY);
        ctx.stroke();
    };
    MarqueeBrush.prototype.drawStroke = function (stroke, inkCanvas) {
        var firstPoint = stroke.points[0];
        var lastPoint = stroke.points[stroke.points.length - 1];
        this._startX = firstPoint.x - inkCanvas._scrollOffset.x + stroke.documentOffsetX;
        this._startY = firstPoint.y - inkCanvas._scrollOffset.y + stroke.documentOffsetY;
        this.draw(lastPoint.x - inkCanvas._scrollOffset.x + stroke.documentOffsetX, lastPoint.y - inkCanvas._scrollOffset.y + stroke.documentOffsetY, inkCanvas);
    };
    return MarqueeBrush;
})();
var MultiSelectionBrush = (function () {
    function MultiSelectionBrush(rect, toRemove) {
        this._rectlist = rect;
        this._list = new Array();
        this._remList = toRemove;
        console.log("new Brush!!!!");
    }
    MultiSelectionBrush.prototype.init = function (x, y, inkCanvas) {
        // do nothing
    };
    MultiSelectionBrush.prototype.draw = function (x, y, inkCanvas) {
        // do nothing.
    };
    MultiSelectionBrush.prototype.setRectList = function (rectList) {
        this._clientRectList = rectList;
    };
    MultiSelectionBrush.prototype.drawStroke = function (stroke, inkCanvas) {
        console.log("draw Stroke =========================================");
        var ctx = inkCanvas._context;
        ctx.globalCompositeOperation = "source-over";
        ctx.globalAlpha = 0.6;
        ctx.beginPath();
        ctx.fillStyle = "rgb(255,255,204)";
        console.log(this._rectlist);
        for (var i = 0; i < this._remList.length; i++) {
            var el = this._remList[i];
            ctx.clearRect(el.left, el.top, el.width, el.height);
        }
        for (var i = 0; i < this._rectlist.length; i++) {
            var el = this._rectlist[i];
            ctx.fillRect(el.left, el.top, el.width, el.height);
        }
        ctx.fill();
    };
    return MultiSelectionBrush;
})();
//var diff = this._rectlist.length - this._remList.length;
//if (diff > 0) {
//    console.log("add!!!!");
//    var rec = this._remList[this._remList.length - 1];
//    if (rec != null) {
//        ctx.clearRect(rec.left, rec.top, rec.width, rec.height);
//        var newRec = this._rectlist[this._remList.length - 1];
//        ctx.fillRect(newRec.left, newRec.top, newRec.width, newRec.height);
//    }
//    console.log("===========!!!!" + diff);
//    $(this._rectlist).each((indx, elem) => {
//        console.log("====================" + indx);
//        console.log(this._remList.length - 1);
//        if (indx >= this._remList.length - 1) {
//            console.log(elem);
//            var x = elem.clientLeft;
//            var y = elem.clientTop;
//            var w = elem.clientWidth;
//            var h = elem.clientHeight;
//            ctx.fillRect(x, y, w, h);
//            console.log("DRAWN");
//        }
//    });
//}
//else if (diff < 0) {
//    console.log("must remove");
//}
//else {
//    console.log("remove last & add last");
//    var rec = this._remList[this._remList.length - 1];
//    ctx.clearRect(rec.left, rec.top, rec.width, rec.height);
//    var newrec = this._rectlist[this._rectlist.length - 1];
//    ctx.fillRect(newrec.left, newrec.top, newrec.width, newrec.height);
//}
//for (var i = 0; i < this._rectlist.length; i++) {
//        var startX = this._rectlist[i].left;
//        var startY = this._rectlist[i].top;
//        var w = this._rectlist[i].width;
//        var h = this._rectlist[i].height;
//        var rect = new Rectangle(startX, startY, w, h);
//        console.log(rect);
//        var count = 0;
//        for (var j = 0; j < this._remList.length; j++) {
//            if (this._remList[j].x == rect.x && this._remList[j].y == rect.y && this._remList[j].w == rect.w && this._remList[j].h == rect.h) {
//                console.log("==========REMOVEREMOVEREMOVE====================");
//                count++;
//            }
//        }
//        if (count==0) {
//            ctx.fillRect(startX, startY, w, h);
//        }
//        else {
//            console.log("=====================RECTLISTREMOVED============================");
//            console.log(rect);
//        }
//}
//for (var i = 0; i < this._remList.length; i++) {
//    var startX = this._remList[i].left;
//    var startY = this._remList[i].top;
//    var w = this._remList[i].width;
//    var h = this._remList[i].height;
//    ctx.clearRect(startX, startY, w, h);
//  ctx.fill();
// this._list = new Array<ClientRect>(); 
var GestureType;
(function (GestureType) {
    GestureType[GestureType["Null"] = 0] = "Null";
    GestureType[GestureType["Diagonal"] = 1] = "Diagonal";
    GestureType[GestureType["Vertical"] = 2] = "Vertical";
    GestureType[GestureType["Horizontal"] = 3] = "Horizontal";
    GestureType[GestureType["Scribble"] = 4] = "Scribble";
})(GestureType || (GestureType = {}));
/// <reference path="../util/Rectangle.ts"/>
var Stroke = (function () {
    function Stroke() {
        this.documentOffsetX = 0;
        this.documentOffsetY = 0;
        this.points = new Array();
    }
    Stroke.fromPoints = function (points) {
        var stroke = new Stroke();
        stroke.points = points.slice(0);
        return stroke;
    };
    Stroke.prototype.getBoundingRect = function () {
        var minX = 1000000;
        var maxX = -1000000;
        var minY = 1000000;
        var maxY = -1000000;
        for (var i = 0; i < this.points.length; i++) {
            var p = this.points[i];
            maxY = p.y > maxY ? p.y : maxY;
            maxX = p.x > maxX ? p.x : maxX;
            minX = p.x < minX ? p.x : minX;
            minY = p.y < minY ? p.y : minY;
        }
        return new Rectangle(minX, minY, maxX - minX, maxY - minY);
    };
    Stroke.prototype.breakUp = function () {
        var segments = new Array();
        for (var i = 1; i < this.points.length; i++) {
            var p0 = this.points[i - 1];
            var p1 = this.points[i];
            segments.push(Line.fromPoint(p0, p1));
        }
        return segments;
    };
    Stroke.prototype.getResampled = function (samples) {
        var c = this.getCopy();
        c.resample(samples);
        return c;
    };
    Stroke.prototype.getEntropy = function () {
        var angles = [];
        for (var i = 1; i < this.points.length; i++) {
            var v0 = new Vector2(this.points[i - 1].x, this.points[i - 1].y);
            var v1 = new Vector2(this.points[i].x, this.points[i].y);
            angles.push(v0.angleTo(v1));
        }
    };
    Stroke.prototype.getStrokeMetrics = function () {
        var startPoint = Vector2.fromPoint(this.points[0]);
        var endPoint = Vector2.fromPoint(this.points[this.points.length - 1]);
        var l = endPoint.subtract(startPoint);
        var ln = l.getNormalized();
        var error = 0;
        var errors = [];
        for (var i = 0; i < this.points.length; i++) {
            var a = Vector2.fromPoint(this.points[i]).subtract(startPoint);
            var b = ln.multiply(a.dot(ln));
            var c = a.subtract(b);
            error += Math.abs(c.length());
            errors.push(Math.abs(c.length()));
        }
        function median(values) {
            values.sort(function (a, b) { return a - b; });
            var half = Math.floor(values.length / 2);
            if (values.length % 2)
                return values[half];
            else
                return (values[half - 1] + values[half]) / 2.0;
        }
        var m = median(errors);
        error /= this.points.length;
        return { length: this.points.length, error: m };
    };
    Stroke.prototype.resample = function (numSamples) {
        var oldSamples = this.points;
        var scale = numSamples / oldSamples.length;
        var newSamples = new Array(numSamples);
        var radius = scale > 1 ? 1 : 1 / (2 * scale);
        var startX = oldSamples[0].x;
        var deltaX = oldSamples[oldSamples.length - 1].x - startX;
        for (var i = 0; i < numSamples; ++i) {
            var center = i / scale + (1.0 - scale) / (2.0 * scale);
            var left = Math.ceil(center - radius);
            var right = Math.floor(center + radius);
            var sum = 0;
            var sumWeights = 0;
            for (var k = left; k <= right; k++) {
                var weight = this.g(k - center, scale);
                var index = Math.max(0, Math.min(oldSamples.length - 1, k));
                sum += weight * oldSamples[index].y;
                sumWeights += weight;
            }
            sum /= sumWeights;
            newSamples[i] = { x: startX + i / numSamples * deltaX, y: sum };
        }
        this.points = newSamples.slice(0);
    };
    Stroke.prototype.g = function (x, a) {
        var radius;
        if (a < 1)
            radius = 1.0 / a;
        else
            radius = 1.0;
        if ((x < -radius) || (x > radius))
            return 0;
        else
            return (1 - Math.abs(x) / radius) / radius;
    };
    Stroke.prototype.getCopy = function () {
        var s = new Stroke();
        s.points = this.points.slice(0);
        s.documentOffsetX = this.documentOffsetX;
        s.documentOffsetY = this.documentOffsetY;
        return s;
    };
    return Stroke;
})();
var GestireClassifier = (function () {
    function GestireClassifier() {
    }
    GestireClassifier.getGestureType = function (stroke) {
        var p0 = stroke.points[0];
        var p1 = stroke.points[stroke.points.length - 1];
        var metrics = stroke.getStrokeMetrics();
        if (Math.abs(p1.x - p0.x) < 5 && Math.abs(p1.y - p0.y) < 5) {
            return GestureType.Null;
        }
        //if (metrics.error > 50) {
        //    return GestureType.Scribble;
        //}
        if (Math.abs(p1.y - p0.y) < 20) {
            return GestureType.Horizontal;
        }
        if (Math.abs(p1.x - p0.x) < 20) {
            return GestureType.Vertical;
        }
        if (Math.abs(p1.x - p0.x) > 50 && Math.abs(p1.y - p0.y) > 20) {
            return GestureType.Diagonal;
        }
    };
    return GestireClassifier;
})();
/// <reference path="../../lib/collections.ts"/>
var BracketSelection = (function (_super) {
    __extends(BracketSelection, _super);
    function BracketSelection(inkCanvas, fromActiveStroke) {
        if (fromActiveStroke === void 0) { fromActiveStroke = false; }
        _super.call(this, "BracketSelection");
        this._brushStroke = null;
        this._inkCanvas = inkCanvas;
        if (fromActiveStroke) {
            this._inkCanvas.setBrush(new HighlightBrush());
            var t = this;
            $.each(inkCanvas._activeStroke.stroke.points, function () {
                t._inkCanvas.draw(this.x, this.y);
            });
        }
    }
    BracketSelection.prototype.start = function (x, y) {
        this._inkCanvas.startDrawing(x, y, new HighlightBrush());
    };
    BracketSelection.prototype.update = function (x, y) {
        this._inkCanvas.draw(x, y);
    };
    BracketSelection.prototype.end = function (x, y) {
        this._inkCanvas.endDrawing(x, y);
        this._brushStroke = this._inkCanvas._activeStroke;
        this.analyzeContent();
        this._brushStroke.brush = new SelectionBrush(this.getBoundingRect());
        this._inkCanvas.update();
    };
    BracketSelection.prototype.deselect = function () {
        this._inkCanvas.removeBrushStroke(this._brushStroke);
    };
    BracketSelection.prototype.getBoundingRect = function () {
        var minX = 1000000;
        var maxX = -1000000;
        var minY = 1000000;
        var maxY = -1000000;
        for (var i = 0; i < this._clientRects.length; i++) {
            var p = this._clientRects[i];
            maxY = p.top + p.height > maxY ? p.top + p.height : maxY;
            maxX = p.left + p.width > maxX ? p.left + p.width : maxX;
            minX = p.left < minX ? p.left : minX;
            minY = p.top < minY ? p.top : minY;
        }
        return new Rectangle(minX, minY + window.pageYOffset, maxX - minX, maxY - minY);
    };
    BracketSelection.prototype.analyzeContent = function () {
        var _this = this;
        var stroke = this._brushStroke.stroke;
        var selectionBB = stroke.getBoundingRect();
        selectionBB.w = Main.DOC_WIDTH - selectionBB.x; // TODO: fix this magic number
        var samplingRate = 30;
        var numSamples = 0;
        var totalScore = 0;
        var hitCounter = new collections.Dictionary(function (elem) { return elem.outerHTML.toString(); });
        var elList = [];
        var scoreList = [];
        for (var x = selectionBB.x; x < selectionBB.x + selectionBB.w; x += samplingRate) {
            for (var y = selectionBB.y; y < selectionBB.y + selectionBB.h; y += samplingRate) {
                var hitElem = document.elementFromPoint(x, y);
                if ($(hitElem).height() > selectionBB.h + 50)
                    continue;
                numSamples++;
                //if (($(hitElem).width() * $(hitElem).height()) / (selectionBB.w * selectionBB.h) < 0.1)
                //    continue;
                var score = 1.0 - Math.sqrt((x - selectionBB.x) / selectionBB.w);
                if (elList.indexOf(hitElem) < 0) {
                    elList.push(hitElem);
                    scoreList.push(score);
                }
                else {
                    scoreList[elList.indexOf(hitElem)] += score;
                }
                if (!hitCounter.containsKey(hitElem)) {
                    hitCounter.setValue(hitElem, score);
                }
                else {
                    hitCounter.setValue(hitElem, hitCounter.getValue(hitElem) + score);
                }
                totalScore += score;
            }
        }
        var maxScore = -10000;
        var bestMatch = null;
        hitCounter.forEach(function (k, v) {
            if (v > maxScore) {
                maxScore = v;
                bestMatch = k;
            }
        });
        var candidates = [];
        var precision = 4;
        hitCounter.forEach(function (k, v) {
            candidates.push(v);
        });
        var std = Statistics.getStandardDeviation(candidates, precision);
        var maxDev = maxScore - 2 * std;
        var finalCandiates = [];
        hitCounter.forEach(function (k, v) {
            if (v >= maxDev && v <= maxScore) {
                finalCandiates.push(k);
            }
        });
        var selectedElements = finalCandiates.filter(function (candidate) {
            var containedByOtherCandidate = false;
            finalCandiates.forEach(function (otherCandidate) {
                if (candidate != otherCandidate && $(otherCandidate).has(candidate)) {
                    containedByOtherCandidate = true;
                }
            });
            return !containedByOtherCandidate;
        });
        this._clientRects = new Array();
        var result = "";
        selectedElements.forEach(function (el) {
            var range = document.createRange();
            range.selectNodeContents(el);
            var rects = range.getClientRects();
            _this._clientRects = _this._clientRects.concat.apply([], rects);
            result += el.outerHTML;
        });
        console.log(this._clientRects);
        console.log("final candidates");
        console.log(selectedElements);
        this._content = result;
        console.log(this._content);
    };
    BracketSelection.prototype.getContent = function () {
        return this._content;
    };
    return BracketSelection;
})(AbstractSelection);
/// <reference path="../ink/brush/MarqueeBrush.ts" />
var MarqueeSelection = (function (_super) {
    __extends(MarqueeSelection, _super);
    function MarqueeSelection(inkCanvas, fromActiveStroke) {
        if (fromActiveStroke === void 0) { fromActiveStroke = false; }
        _super.call(this, "MarqueeSelection");
        this._startX = 0;
        this._startY = 0;
        this._mouseX = 0;
        this._mouseY = 0;
        this._marqueeX1 = 0;
        this._marqueeY1 = 0;
        this._marqueeX2 = 0;
        this._marqueeY2 = 0;
        this._parentList = new Array();
        this._selected = null;
        this._ct = 0;
        this._content = "";
        this._offsetY = 0;
        this._inkCanvas = inkCanvas;
        if (fromActiveStroke) {
            var stroke = inkCanvas._activeStroke.stroke;
            this._offsetY = stroke.documentOffsetY;
            this._startX = stroke.points[0].x;
            this._startY = stroke.points[0].y;
            this._mouseX = stroke.points[stroke.points.length - 1].x;
            this._mouseY = stroke.points[stroke.points.length - 1].y;
            this._ct = 0;
            this._marqueeX1 = this._startX;
            this._marqueeX2 = this._mouseX;
            this._marqueeY1 = this._startY + $(window).scrollTop();
            this._marqueeY2 = this._mouseY + $(window).scrollTop();
            inkCanvas.setBrush(new MarqueeBrush(this._startX, this._startY));
        }
    }
    MarqueeSelection.prototype.start = function (x, y) {
        this._inkCanvas.startDrawing(x, y, new MarqueeBrush(x, y));
        this._parentList = [];
        this._offsetY = window.pageYOffset;
        this._startX = x;
        this._startY = y;
        this._mouseX = x;
        this._mouseY = y;
    };
    MarqueeSelection.prototype.update = function (x, y) {
        this._mouseX = x;
        this._mouseY = y;
        this._marqueeX1 = this._startX;
        this._marqueeY1 = this._startY;
        this._marqueeX2 = this._mouseX;
        this._marqueeY2 = this._mouseY;
        this._inkCanvas.update();
        this._inkCanvas.draw(x, y);
    };
    MarqueeSelection.prototype.end = function (x, y) {
        var el = document.elementFromPoint(this._startX, this._startY);
        this._parentList.push(el);
        this._selected = el;
        if (this._marqueeX1 > this._marqueeX2) {
            var temp = this._marqueeX1;
            this._marqueeX1 = this._marqueeX2;
            this._marqueeX2 = temp;
        }
        if (this._marqueeY1 > this._marqueeY2) {
            var temp = this._marqueeY1;
            this._marqueeY1 = this._marqueeY2;
            this._marqueeY2 = temp;
        }
        //finds the common parent of all elements in selection range
        if (el != null) {
            this.getNextElement(el);
        }
        this._inkCanvas.endDrawing(x, y);
        this._brushStroke = this._inkCanvas._activeStroke;
        //        this._brushStroke.brush = new SelectionBrush(this.getBoundingRect());
        this._inkCanvas.update();
        this.analyzeContent();
    };
    MarqueeSelection.prototype.deselect = function () {
        this._inkCanvas.removeBrushStroke(this._brushStroke);
    };
    MarqueeSelection.prototype.getNextElement = function (el) {
        //recursively adds elements in selection range to parentList. 
        if (this._selected != el) {
            return;
        }
        if (this._ct == 50) {
            throw new Error("an exception! please add to edge case list!");
        }
        this._ct++;
        var rect = el.getBoundingClientRect();
        var nextX = this._mouseX - (rect.left + rect.width);
        var nextY = this._mouseY - (rect.top + rect.height);
        var newList = [];
        if (el.nodeName == "HTML") {
            return;
        }
        if (nextX > 0) {
            if (document.body.contains(this._inkCanvas._canvas)) {
                document.body.removeChild(this._inkCanvas._canvas);
            }
            if (!this.isDescendant(el, document.elementFromPoint(this._mouseX - nextX + 1, this._startY))) {
                var element = document.elementFromPoint(this._mouseX - nextX + 1, this._startY);
                for (var i = 0; i < this._parentList.length; i++) {
                    if (this.isDescendant(element, this._parentList[i])) {
                    }
                    else {
                        newList.push(this._parentList[i]);
                    }
                }
                this._selected = element;
                this.drawPreviousMarquee();
                this._startX = this._mouseX - nextX + 1;
                this._parentList = newList;
                this._parentList.push(element);
                this.getNextElement(element);
            }
        }
        if (nextY > 0) {
            if (document.body.contains(this._inkCanvas._canvas)) {
                document.body.removeChild(this._inkCanvas._canvas);
            }
            element = document.elementFromPoint(this._startX, this._mouseY - nextY + 1);
            var contains = false;
            for (var i = 0; i < this._parentList.length; i++) {
                if (this.isDescendant(this._parentList[i], element) || this._parentList[i] == element) {
                    contains = true;
                }
            }
            if (contains) {
                this.drawPreviousMarquee();
                return;
            }
            for (var i = 0; i < this._parentList.length; i++) {
                if (this.isDescendant(element, this._parentList[i])) {
                }
                else {
                    newList.push(this._parentList[i]);
                }
            }
            this._selected = element;
            this._startY = this._mouseY - nextY + 1;
            this._startX = this._marqueeX1;
            this._parentList = newList;
            this._parentList.push(element);
            this.drawPreviousMarquee();
            this.getNextElement(element);
        }
    };
    MarqueeSelection.prototype.isDescendant = function (parent, child) {
        var node = child.parentNode;
        while (node != null) {
            if (node == parent) {
                return true;
            }
            node = node.parentNode;
        }
        return false;
    };
    MarqueeSelection.prototype.drawPreviousMarquee = function () {
        var canvas = this._inkCanvas._canvas;
        var ctx = this._inkCanvas._context;
        document.body.appendChild(canvas);
        this._inkCanvas.update();
        this._inkCanvas.draw(this._marqueeX2, this._marqueeY2);
    };
    MarqueeSelection.prototype.getBoundingRect = function () {
        return new Rectangle(this._marqueeX1, this._offsetY + this._marqueeY1, this._marqueeX2 - this._marqueeX1, this._marqueeY2 - this._marqueeY1);
    };
    MarqueeSelection.prototype.analyzeContent = function () {
        if (this._parentList.length != 1) {
            for (var i = 1; i < this._parentList.length; i++) {
                var currAn = this.commonAncestor(this._parentList[0], this._parentList[i]);
                this._parentList[0] = currAn;
            }
        }
        var sel = this._parentList[0].cloneNode(true);
        var selX = $(this._parentList[0]).clone(true);
        this.rmChildNodes(sel, this._parentList[0]);
        var htmlString = sel.innerHTML.replace(/"/g, "'");
        if (sel.outerHTML == "") {
            this._content = sel.innerHTML;
        }
        this._content = sel.outerHTML;
    };
    MarqueeSelection.prototype.commonAncestor = function (node1, node2) {
        //finds common ancestor between two nodes. 
        var parents1 = this.parents(node1);
        var parents2 = this.parents(node2);
        if (parents1[0] != parents2[0]) {
            throw "No common ancestor!";
        }
        for (var i = 0; i < parents1.length; i++) {
            if (parents1[i] != parents2[i]) {
                return parents1[i - 1];
            }
        }
        return parents1[parents1.length - 1];
    };
    MarqueeSelection.prototype.parents = function (node) {
        var nodes = [node];
        while (node != null) {
            node = node.parentNode;
            nodes.unshift(node);
        }
        return nodes;
    };
    MarqueeSelection.prototype.bound = function (myEl, el) {
        if (el.nodeName == "#text") {
            var range = document.createRange();
            range.selectNodeContents(el);
            var rects = range.getClientRects();
            if (rects.length == 0) {
                return;
            }
            for (var i = 0; i < rects.length; i++) {
                var ax1 = rects[i].left;
                var ax2 = rects[i].left + rects[0].width;
                var ay1 = rects[i].top;
                var ay2 = rects[i].top + rects[0].height;
                if (!(ax1 >= this._marqueeX1 &&
                    ax2 <= this._marqueeX2 &&
                    ay1 >= this._marqueeY1 &&
                    ay2 <= this._marqueeY2)) {
                    return false;
                }
            }
            return true;
        }
        else if (el.nodeName != "#comment") {
            var rectX = el.getBoundingClientRect();
            var realDim = this.getRealHeightWidth(el.getClientRects());
            var realHeight = realDim[0];
            var realWidth = realDim[1];
            if (rectX == null) {
                return false;
            }
            if (rectX["left"] >= this._marqueeX1 &&
                rectX["left"] + realWidth <= this._marqueeX2 &&
                rectX["top"] >= this._marqueeY1 &&
                rectX["top"] + realHeight <= this._marqueeY2) {
                this.setTextStyle(myEl, el);
                return true;
            }
            return false;
        }
    };
    MarqueeSelection.prototype.rmChildNodes = function (el, trueEl) {
        var removed = [];
        var realNList = [];
        var indexList = [];
        //iterate through childNodes and add to list(removed).
        for (var i = 0; i < el.childNodes.length; i++) {
            if (!this.intersectWith(el.childNodes[i], trueEl.childNodes[i])) {
                removed.push(el.childNodes[i]);
            }
            else {
                realNList.push(trueEl.childNodes[i]);
                indexList.push(i);
            }
        }
        //remove not intersecting elements; 
        for (var i = 0; i < removed.length; i++) {
            el.removeChild(removed[i]);
        }
        for (var i = 0; i < el.childNodes.length; i++) {
            if (!this.bound(el.childNodes[i], realNList[i])) {
                if (el.childNodes[i].nodeName == "#text") {
                    var index = indexList[i];
                    $(trueEl.childNodes[indexList[i]]).replaceWith("<span>" + $(trueEl.childNodes[indexList[i]]).text().replace(/([^\s]*)/g, "<word>$1</word>") + "</span>");
                    var result = "";
                    for (var j = 0; j < trueEl.childNodes[indexList[i]].childNodes.length; j++) {
                        if (this.intersectWith(trueEl.childNodes[index].childNodes[j], trueEl.childNodes[index].childNodes[j])) {
                            //   console.log((trueEl.childNodes[index].childNodes[j]));
                            // console.log("YELLOW!!!!!!!!!!!!");
                            if (trueEl.childNodes[index].childNodes[j].style) {
                                trueEl.childNodes[index].childNodes[j].style.backgroundColor = "yellow";
                            }
                            //else {
                            //   var wrap = document.createElement('span');
                            //   wrap.appendChild(trueEl.childNodes[index].childNodes[j]);
                            //    wrap.style.backgroundColor = "yellow";
                            //}
                            if (!trueEl.childNodes[index].childNodes[j]["innerHTML"]) {
                                if (trueEl.childNodes[index].childNodes[j].nodeName == "WORD") {
                                    result += " ";
                                }
                            }
                            else {
                                result += trueEl.childNodes[index].childNodes[j]["innerHTML"];
                            }
                        }
                    }
                    el.childNodes[i].data = result;
                }
                else {
                    this.rmChildNodes(el.childNodes[i], realNList[i]);
                }
            }
            else {
                console.log(realNList[i]);
                if (realNList[i].nodeName == "#text") {
                    $(trueEl.childNodes[indexList[i]]).replaceWith("<word>" + $(realNList[i]).text() + "</word>");
                    console.log("=====================");
                }
                //$(realNList[i]).css("background-color", "yellow"); 
                trueEl.childNodes[indexList[i]].style.backgroundColor = "yellow";
                //if (trueEl.childNodes[index].childNodes[j]) {
                //    trueEl.childNodes[index].childNodes[j].style.backgroundColor = "yellow";
                //}
                console.log("!!!!!!!!!!!!!!!!!!!!!!BOUNDDED");
            }
        }
    };
    MarqueeSelection.prototype.setTextStyle = function (el, trueEl) {
        var elStyle = document.defaultView.getComputedStyle(trueEl["parentElement"]);
        el = el.parentNode;
        el.style.font = elStyle.font;
    };
    MarqueeSelection.prototype.getRealHeightWidth = function (rectsList) {
        //finds the real Heights and Widths of DOM elements by iterating through their clientRectList.
        var maxH = Number.NEGATIVE_INFINITY;
        var minH = Number.POSITIVE_INFINITY;
        var maxW = Number.NEGATIVE_INFINITY;
        var minW = Number.POSITIVE_INFINITY;
        $(rectsList).each(function (indx, elem) {
            if (elem["top"] + elem["height"] > maxH) {
                maxH = elem["top"] + elem["height"];
            }
            if (elem["top"] < minH) {
                minH = elem["top"];
            }
            if (elem["left"] < minW) {
                minW = elem["left"];
            }
            if (elem["left"] + elem["width"] > maxW) {
                maxW = elem["left"] + elem["width"];
            }
        });
        return [(maxH - minH), (maxW - minW), minW, minH];
    };
    MarqueeSelection.prototype.intersectWith = function (myEl, el) {
        //checks if element is intersecting with selection range 
        if (!el) {
            return false;
        }
        ;
        var bx1 = this._marqueeX1;
        var bx2 = this._marqueeX2;
        var by1 = this._marqueeY1;
        var by2 = this._marqueeY2;
        if (el.nodeName == "#text") {
            // this.setTextStyle(myEl, el);                        
            var range = document.createRange();
            range.selectNodeContents(el);
            var rects = range.getClientRects();
            if (rects.length == 0) {
                return;
            }
            for (var i = 0; i < rects.length; i++) {
                var ax1 = rects[i].left;
                var ax2 = rects[i].left + rects[i].width;
                var ay1 = rects[i].top;
                var ay2 = rects[i].top + rects[i].height;
                if (ax1 < bx2 && ax2 > bx1 && ay1 < by2 && ay2 > by1) {
                    return true;
                }
            }
            return false;
        }
        else if (el.nodeName != "#comment") {
            var rangeY = document.createRange();
            rangeY.selectNodeContents(el);
            var realDim = this.getRealHeightWidth(rangeY.getClientRects());
            var realHeight = realDim[0];
            var realWidth = realDim[1];
            var minX = realDim[2];
            var minY = realDim[3];
            /////works weird for Wikipedia. 
            ax1 = el.getBoundingClientRect()["left"];
            ax2 = el.getBoundingClientRect()["left"] + realWidth;
            ay1 = el.getBoundingClientRect()["top"];
            ay2 = el.getBoundingClientRect()["top"] + realHeight;
        }
        if (ax1 < bx2 && bx1 < ax2 && ay1 < by2) {
            return by1 < ay2;
        }
        else {
            return false;
        }
    };
    MarqueeSelection.prototype.getContent = function () {
        return this._content;
    };
    return MarqueeSelection;
})(AbstractSelection);
var MultiLineSelection = (function (_super) {
    __extends(MultiLineSelection, _super);
    function MultiLineSelection(inkCanvas, fromActiveStroke) {
        var _this = this;
        if (fromActiveStroke === void 0) { fromActiveStroke = false; }
        _super.call(this, "MultiLineSelection");
        this.getTextRectangles = function (cont, nEnd) {
            console.log(cont.childNodes);
            $(cont.childNodes).each(function (index, el) {
                console.log(el);
                console.log(el.nodeName);
                if (el.nodeName == "#text") {
                    var range = document.createRange();
                    range.selectNodeContents(el);
                    console.log(range);
                    console.log(range.getClientRects());
                    console.log(range.getBoundingClientRect());
                }
            });
            return new Array();
        };
        this.getNodesInRange = function (range) {
            var start = range.startContainer;
            var end = range.endContainer;
            var commonAncestor = range.commonAncestorContainer;
            var nodes = [];
            var node;
            // walk parent nodes from start to common ancestor
            for (node = start.parentNode; node; node = node.parentNode) {
                nodes.push(node);
                if (node == commonAncestor)
                    break;
            }
            nodes.reverse();
            // walk children and siblings from start until end is found
            for (node = start; node; node = _this.getNextNode(node)) {
                nodes.push(node);
                if (node == end)
                    break;
            }
            return nodes;
        };
        this.getNextNode = function (node) {
            if (node.firstChild)
                return node.firstChild;
            while (node) {
                if (node.nextSibling)
                    return node.nextSibling;
                node = node.parentNode;
            }
        };
        this.isDescendant = function (parent, child) {
            var node = child.parentNode;
            while (node != null) {
                if (node == parent) {
                    return true;
                }
                node = node.parentNode;
            }
            return false;
        };
        this.getTextNodesBetween = function (range) {
            var rootNode = range.commonAncestorContainer, startNode = range.startContainer, endNode = range.endContainer, startOffset = range.startOffset, endOffset = range.endOffset, pastStartNode = false, reachedEndNode = false, textNodes = [];
            function getTextNodes(node) {
                var val = node.nodeValue;
                if (node == startNode && node == endNode && node !== rootNode) {
                    if (val)
                        textNodes.push(node);
                    console.log(node);
                    pastStartNode = reachedEndNode = true;
                }
                else if (node == startNode) {
                    if (val)
                        textNodes.push(node);
                    pastStartNode = true;
                    console.log(node);
                }
                else if (node == endNode) {
                    if (val)
                        textNodes.push(node);
                    reachedEndNode = true;
                    console.log(node);
                }
                else if (node.nodeType == 3) {
                    if (val && pastStartNode && !reachedEndNode && !/^\s*$/.test(val)) {
                        //    textNodes.push(val);
                        textNodes.push(node);
                        console.log(node);
                    }
                }
                //else if (node.nodeName == "IMG") {
                //    //list.push(node);
                //    addEventLis
                //}
                for (var i = 0, len = node.childNodes.length; !reachedEndNode && i < len; ++i) {
                    getTextNodes(node.childNodes[i]);
                }
            }
            getTextNodes(rootNode);
            return textNodes;
        };
        this._brushStroke = null;
        this._inkCanvas = inkCanvas;
        this._rectList = new Array();
        this._currLineTop = 0;
        console.log("===============constructor============");
        if (fromActiveStroke) {
            this._inkCanvas.setBrush(new HighlightBrush());
            var t = this;
            $.each(inkCanvas._activeStroke.stroke.points, function () {
                t._inkCanvas.draw(this.x, this.y);
            });
        }
    }
    MultiLineSelection.prototype.addWordTag = function (nodes) {
        var _this = this;
        console.log(nodes);
        $.each(nodes, function (index, value) {
            if (value.nodeType == Node.TEXT_NODE) {
                $(value).replaceWith("<span>" + $(value).text().replace(/([^\s]*)/g, "<word>$1</word>") + "</span>");
            }
            else if (value.childNodes.length > 0) {
                _this.addWordTag(value.childNodes);
            }
        });
    };
    MultiLineSelection.prototype.start = function (x, y) {
        console.log("===================start===============");
        document.body.removeChild(this._inkCanvas._canvas);
        console.log(document.elementFromPoint(x, y));
        //      this.addWordTag(document.elementFromPoint(x, y).childNodes);
        this._currParent = document.elementFromPoint(x, y);
        var rg = document["caretRangeFromPoint"](x, y);
        this._nStart = rg.commonAncestorContainer;
        this._offsetStart = rg.startOffset;
        console.log(this._offsetStart);
        this._prevList = Array();
        this._startX = x;
        this._startY = y;
    };
    MultiLineSelection.prototype.update = function (x, y) {
        if (this._startX == x && this._startY == y) {
            console.log("=========================!!!==");
            return;
        }
        this._inkCanvas.draw(x, y);
        document.body.removeChild(this._inkCanvas._canvas);
        var rg = document["caretRangeFromPoint"](x, y);
        var nEnd = rg.commonAncestorContainer;
        var offsetEnd = rg.startOffset;
        //var offsetStart = this._offsetStart;
        //this._nEnd = nEnd;
        this._range = document.createRange();
        this._range.setStart(this._nStart, this._offsetStart);
        this._range.setEnd(nEnd, offsetEnd);
        //this._inkCanvas.draw(x, y);
        //var rg = document["caretRangeFromPoint"](x, y);
        //var nEnd = rg.commonAncestorContainer;
        //var offsetEnd = rg.startOffset;
        //var offsetStart = this._offsetStart;
        //this._nEnd = nEnd;
        //this._range = document.createRange();
        //this._range.setStart(this._nStart, this._offsetStart);
        //this._range.setEnd(nEnd, offsetEnd);
        //var ans = this._range.commonAncestorContainer;
        //var nodes = this.getTextNodesBetween(this._range);
        //var list = [];
        //$(nodes).each(function (indx, ele) {
        //    var rg = document.createRange();
        //    if (indx == 0) {
        //        if ($(nodes).length == 1) {
        //            rg.setStart(ele, offsetStart);
        //            rg.setEnd(ele, offsetEnd);
        //        }
        //        else {
        //                rg.setStart(ele, offsetStart);
        //                rg.setEndAfter(ele);
        //        }
        //    }
        //    else if (indx == $(nodes).length - 1) {
        //        rg.setStartBefore(ele);
        //        rg.setEnd(ele, offsetEnd);
        //    }
        //    else {
        //        rg.selectNode(ele);
        //    }
        //    console.log(rg.getClientRects());
        //    $(rg.getClientRects()).each(function (idx, el) {
        //        list.push(el);
        //    });
        //});
        //this._brushStroke = this._inkCanvas._activeStroke;
        //this._brushStroke.brush = new MultiSelectionBrush(list, this._prevList);
        //this._brushStroke.brush.drawStroke(null, this._inkCanvas);
        //this._prevList = list;
        //if (this._prevList == null) {
        //    console.log("prev is null");
        //    this._brushStroke.brush = new MultiSelectionBrush(list, []);
        //}
        //else if (list.length > this._prevList.length) {
        //    ///delete last element of prevlist and add 
        //    console.log("more clientrect selected");
        //    var diff = list.length - this._prevList.length;
        //    this._brushStroke.brush = new MultiSelectionBrush(list.slice(list.length - diff), [this._prevList[this._prevList.length - 1]]);
        //}
        //else if (list.length < this._prevList.length) {
        //    ////remove previous and check last 
        //    console.log("less clientrect selected!!!");
        //}
        //else {
        //    ////check the last rect 
        //    console.log("selection within same rect");
        //    this._brushStroke.brush = new MultiSelectionBrush([list[list.length - 1]], [this._prevList[this._prevList.length - 1]]);
        //}
    };
    MultiLineSelection.prototype.end = function (x, y) {
        this._inkCanvas.endDrawing(x, y);
        //this._brushStroke = this._inkCanvas._activeStroke;
        this.analyzeContent();
        //  this._brushStroke.brush = new SelectionBrush(this.getBoundingRect());
        this._inkCanvas.update();
    };
    MultiLineSelection.prototype.deselect = function () {
        this._inkCanvas.removeBrushStroke(this._brushStroke);
    };
    MultiLineSelection.prototype.getBoundingRect = function () {
        return new Rectangle(1, 10, 10, 10);
    };
    MultiLineSelection.prototype.analyzeContent = function () {
        var content = this._range.cloneContents();
        console.log(content);
    };
    MultiLineSelection.prototype.getContent = function () {
        // console.log("getContent =======================");
        var d = document.createElement('div');
        d.appendChild(this._range.cloneContents());
        console.log(d.innerHTML);
        return d.innerHTML;
        //return this._range.cloneContents();
    };
    return MultiLineSelection;
})(AbstractSelection);
var GestureType;
(function (GestureType) {
    GestureType[GestureType["Null"] = 0] = "Null";
    GestureType[GestureType["Diagonal"] = 1] = "Diagonal";
    GestureType[GestureType["Vertical"] = 2] = "Vertical";
    GestureType[GestureType["Horizontal"] = 3] = "Horizontal";
    GestureType[GestureType["Scribble"] = 4] = "Scribble";
})(GestureType || (GestureType = {}));
var Line = (function () {
    function Line() {
    }
    Line.fromPoint = function (start, end) {
        var line = new Line();
        line.start = new Vector2(start.x, start.y);
        line.end = new Vector2(end.x, end.y);
        return line;
    };
    Line.fromVector = function (start, end) {
        var line = new Line();
        line.start = start.clone();
        line.end = end.clone();
        return line;
    };
    Line.prototype.intersectsLine = function (other) {
        var s1_x = this.end.x - this.start.x;
        var s1_y = this.end.y - this.start.y;
        var s2_x = other.end.x - other.start.x;
        var s2_y = other.end.y - other.start.y;
        var s, t;
        s = (-s1_y * (this.start.x - other.start.x) + s1_x * (this.start.y - other.start.y)) / (-s2_x * s1_y + s1_x * s2_y);
        t = (s2_x * (this.start.y - other.start.y) - s2_y * (this.start.x - other.start.x)) / (-s2_x * s1_y + s1_x * s2_y);
        if (s >= 0 && s <= 1 && t >= 0 && t <= 1) {
            return true;
        }
        return false; // No collision
    };
    return Line;
})();
var Statistics = (function () {
    function Statistics() {
    }
    Statistics.getNumWithSetDec = function (num, numOfDec) {
        var pow10s = Math.pow(10, numOfDec || 0);
        return (numOfDec) ? Math.round(pow10s * num) / pow10s : num;
    };
    Statistics.getAverageFromNumArr = function (numArr, numOfDec) {
        var i = numArr.length, sum = 0;
        while (i--) {
            sum += numArr[i];
        }
        return Statistics.getNumWithSetDec((sum / numArr.length), numOfDec);
    };
    Statistics.getVariance = function (numArr, numOfDec) {
        var avg = Statistics.getAverageFromNumArr(numArr, numOfDec), i = numArr.length, v = 0;
        while (i--) {
            v += Math.pow((numArr[i] - avg), 2);
        }
        v /= numArr.length;
        return Statistics.getNumWithSetDec(v, numOfDec);
    };
    Statistics.isWithinStd = function (num, dist, std) {
        return num >= dist * std;
    };
    Statistics.getStandardDeviation = function (numArr, numOfDec) {
        var stdDev = Math.sqrt(Statistics.getVariance(numArr, numOfDec));
        return Statistics.getNumWithSetDec(stdDev, numOfDec);
    };
    return Statistics;
})();
var Vector2 = (function () {
    function Vector2(x, y) {
        this.x = x;
        this.y = y;
    }
    Vector2.fromPoint = function (p) {
        return new Vector2(p.x, p.y);
    };
    Vector2.prototype.negative = function () {
        return new Vector2(-this.x, -this.y);
    };
    Vector2.prototype.add = function (v) {
        if (v instanceof Vector2)
            return new Vector2(this.x + v.x, this.y + v.y);
        else
            return new Vector2(this.x + v, this.y + v);
    };
    Vector2.prototype.subtract = function (v) {
        if (v instanceof Vector2)
            return new Vector2(this.x - v.x, this.y - v.y);
        else
            return new Vector2(this.x - v, this.y - v);
    };
    Vector2.prototype.multiply = function (v) {
        if (v instanceof Vector2)
            return new Vector2(this.x * v.x, this.y * v.y);
        else
            return new Vector2(this.x * v, this.y * v);
    };
    Vector2.prototype.divide = function (v) {
        if (v instanceof Vector2)
            return new Vector2(this.x / v.x, this.y / v.y);
        else
            return new Vector2(this.x / v, this.y / v);
    };
    Vector2.prototype.equals = function (v) {
        return this.x == v.x && this.y == v.y;
    };
    Vector2.prototype.dot = function (v) {
        return this.x * v.x + this.y * v.y;
    };
    Vector2.prototype.length = function () {
        return Math.sqrt(this.dot(this));
    };
    Vector2.prototype.getNormalized = function () {
        return this.divide(this.length());
    };
    Vector2.prototype.distanceTo = function (other) {
        return Math.sqrt((this.x - other.x) * (this.x - other.x) + (this.y - other.y) * (this.y - other.y));
    };
    Vector2.prototype.cross = function (other) {
        return this.x * other.y - this.y * other.x;
    };
    Vector2.prototype.clone = function () {
        return new Vector2(this.x, this.y);
    };
    Vector2.prototype.angleTo = function (a) {
        return Math.acos(this.dot(a) / (this.length() * a.length()));
    };
    Vector2.prototype.init = function (x, y) {
        this.x = x;
        this.y = y;
        return this;
    };
    return Vector2;
})();
//# sourceMappingURL=NuSysChromeExtension.js.map