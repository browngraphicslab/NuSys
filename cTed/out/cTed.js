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
var HighlightBrush = (function () {
    function HighlightBrush() {
        this._img = new Image();
        this._img.src = chrome.extension.getURL("assets/brush.png");
    }
    HighlightBrush.prototype.draw = function (x, y, inkCanvas) {
        inkCanvas._context.globalCompositeOperation = "xor";
        inkCanvas._context.globalAlpha = 0.6;
        inkCanvas._context.drawImage(this._img, x - 15, y - 15, 30, 30);
    };
    HighlightBrush.prototype.focusLine = function (line, inkCanvas) {
    };
    HighlightBrush.prototype.redraw = function (stroke, inkCanvas) {
    };
    HighlightBrush.prototype.focusPoint = function (p, inkCanvas) {
        var canvas = inkCanvas._canvas;
        var ctx = inkCanvas._context;
        ctx.globalCompositeOperation = "source-over";
        ctx.fillStyle = '#0000FF';
        ctx.beginPath();
        ctx.moveTo(p.x, p.y);
        ctx.arc(p.x, p.y, 8, 0, Math.PI * 2, false);
        ctx.fill();
    };
    HighlightBrush.prototype.drawPrevious = function (stroke, inkCanvas) { };
    return HighlightBrush;
})();
var Point = (function () {
    function Point(x, y) {
        this.x = x;
        this.y = y;
    }
    return Point;
})();
/// <reference path = "util/Point.ts"/>
var Stroke = (function () {
    function Stroke() {
        this.points = new Array();
    }
    Stroke.prototype.push = function (x, y) {
        this.points.push({ x: x, y: y });
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
    Stroke.prototype.nearestPointArea = function (p) {
        console.log("nearestpont for " + p.x + "and " + p.y);
        var xval = Math.floor(p.x / 3);
        var yval = Math.floor(p.y / 3);
        if (Math.abs(p.x / 3 - xval) > 0.5)
            xval++;
        if (Math.abs(p.y / 3 - yval) > 0.5)
            yval++;
        console.log("resulting : " + xval + " and " + yval);
        return new Point(xval, yval);
    };
    Stroke.prototype.degree = function (p1, p2) {
        return Math.atan2(p2.y - p1.y, p2.x - p1.x) * 180 / Math.PI;
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
    Stroke.prototype.sampleStroke = function () {
        var len = this.points.length;
        var ypre;
        var xpre;
        var predg = 0;
        var prept = this.points[0];
        var strokeHash = {};
        var sampledStrokes = [];
        sampledStrokes.push(prept);
        for (var i = 1; i < len; i++) {
            //    var pt = this.nearestPointArea(this.points[i]);
            var pt = this.points[i];
            if (Math.abs(predg - this.degree(pt, prept)) < 10 && i < len - 1) {
                continue;
            }
            predg = this.degree(pt, prept);
            sampledStrokes.push(this.points[i]);
        }
        var res = new Stroke();
        res.points = sampledStrokes;
        return res;
    };
    return Stroke;
})();
var StrokeType;
(function (StrokeType) {
    StrokeType[StrokeType["Null"] = 0] = "Null";
    StrokeType[StrokeType["Line"] = 1] = "Line";
    StrokeType[StrokeType["Bracket"] = 2] = "Bracket";
    StrokeType[StrokeType["Marquee"] = 3] = "Marquee";
    StrokeType[StrokeType["Lasso"] = 4] = "Lasso";
    StrokeType[StrokeType["MultiLine"] = 5] = "MultiLine";
})(StrokeType || (StrokeType = {}));
/// <reference path="Stroke.ts"/>
/// <reference path="StrokeType.ts"/>
var InkCanvas = (function () {
    function InkCanvas(canvas) {
        this._canvas = canvas;
        this._context = canvas.getContext("2d");
        this._brush = new HighlightBrush();
        this._activeStroke = new Stroke();
        console.log("new Canvas!!!!!");
    }
    InkCanvas.prototype.drawStroke = function (stroke) {
        var sample = stroke.sampleStroke();
        for (var i = 0; i < sample.points.length; i++) {
            this._brush.draw(sample[i].x, sample[i].y, this);
        }
    };
    InkCanvas.prototype.draw = function (x, y) {
        this._activeStroke.push(x, y);
        this._brush.draw(x, y, this);
    };
    InkCanvas.prototype.removeStroke = function () {
        this._context.clearRect(0, 0, this._canvas.width, this._canvas.height);
        //   this.update();
    };
    InkCanvas.prototype.switchBrush = function (strokeType) {
        console.log("INKCANVAS brush switched to : " + strokeType);
        switch (strokeType) {
            //////STROKE TYPE CHANGE 
            case StrokeType.Marquee:
                this._brush = new MarqueeBrush();
                break;
            case StrokeType.Line:
                this._brush = new HighlightBrush();
                break;
            case StrokeType.Lasso:
                this._brush = new LassoBrush();
                break;
            default:
                this._brush = new HighlightBrush();
        }
        this._brush.redraw(this._activeStroke, this);
    };
    InkCanvas.prototype.drawPreviousGestureM = function (stroke) {
        console.log("======================redraw==");
        this._prevBrush = new MarqueeBrush();
        this._prevBrush.drawPrevious(stroke, this);
    };
    InkCanvas.prototype.drawPointsAndLines = function (points) {
        this.clear();
        this.drawPoint(points[0]);
        this.drawline(points[0], points[points.length - 1]);
        for (var i = 1; i < points.length; i++) {
            this.drawPoint(points[i]);
            this.drawline(points[i], points[i - 1]);
        }
    };
    InkCanvas.prototype.drawPreviousGesture = function (sel) {
        this.drawPointsAndLines(sel.samplePoints);
    };
    InkCanvas.prototype.drawPreviousGestureL = function (points) {
        console.log("======================redraw==");
        this._prevBrush = new LassoBrush();
        var stroke = new Stroke();
        stroke.points = points;
        this._prevBrush.drawPrevious(stroke, this);
    };
    InkCanvas.prototype.drawPoint = function (p) {
        var ctx = this._context;
        ctx.globalCompositeOperation = "source-over";
        ctx.fillStyle = '#ff0000';
        ctx.beginPath();
        ctx.moveTo(p.x, p.y);
        ctx.arc(p.x, p.y, 5, 0, Math.PI * 2, false);
        ctx.fill();
    };
    InkCanvas.prototype.drawline = function (p1, p2) {
        var ctx = this._context;
        ctx.lineWidth = 1;
        ctx.strokeStyle = '#123456';
        ctx.setLineDash([]);
        ctx.beginPath();
        ctx.moveTo(p1.x, p1.y);
        ctx.lineTo(p2.x, p2.y);
        ctx.stroke();
    };
    InkCanvas.prototype.editPoint = function (points, e) {
        var sampleStroke = points;
        var lines = [];
        for (var i = 1; i < sampleStroke.length; i++) {
            if (Math.abs(e.clientX - sampleStroke[i].x) < 3 && Math.abs(e.clientY - sampleStroke[i].y) < 3) {
                this.focusPoint(sampleStroke[i]);
                return sampleStroke[i];
            }
        }
    };
    InkCanvas.prototype.editStrokes = function (points, e) {
        var sampleStroke = points;
        var lines = [];
        for (var i = 0; i < sampleStroke.length; i++) {
            console.log("DAXXXXXXXXF");
            if (i == 0) {
                var line = new Line(sampleStroke[sampleStroke.length - 1], sampleStroke[0]);
            }
            else {
                var line = new Line(sampleStroke[i - 1], sampleStroke[i]);
            }
            if (this.checkAboveLine(line, new Point(e.clientX, e.clientY))) {
                this.focusLine(line);
                return line;
            }
        }
    };
    InkCanvas.prototype.focusLine = function (line) {
        var ctx = this._context;
        ctx.beginPath();
        //  ctx.fillStyle = '#ff0000';
        ctx.moveTo(line.p1.x, line.p1.y);
        ctx.lineTo(line.p2.x, line.p2.y);
        ctx.lineWidth = 3;
        ctx.stroke();
    };
    InkCanvas.prototype.focusPoint = function (p) {
        var ctx = this._context;
        ctx.globalCompositeOperation = "source-over";
        ctx.fillStyle = '#0000FF';
        ctx.beginPath();
        ctx.moveTo(p.x, p.y);
        ctx.arc(p.x, p.y, 8, 0, Math.PI * 2, false);
        ctx.fill();
    };
    InkCanvas.prototype.isBetween = function (a, b, x) {
        if (a > b) {
            return x < a && x > b;
        }
        else {
            return x < b && x > a;
        }
    };
    InkCanvas.prototype.checkAboveLine = function (line, mouse) {
        if (line.p1.x == line.p2.x) {
            return (Math.abs(mouse.x - line.p1.x) < 5 && this.isBetween(line.p1.y, line.p2.y, mouse.y));
        }
        if (line.p1.y == line.p2.y) {
            return (Math.abs(mouse.y - line.p1.y) < 5 && this.isBetween(line.p1.x, line.p2.x, mouse.x));
        }
        var m1 = (mouse.y - line.p1.y) / (mouse.x - line.p1.x);
        var m2 = (line.p2.y - mouse.y) / (line.p2.x - mouse.x);
        // console.log((m1 == m2) && (line.p1.y <= mouse.y && mouse.y <= line.p2.y) && (line.p1.x <= mouse.x && mouse.x <= line.p2.x));
        return (Math.abs(m1 - m2) < 0.15) && (this.isBetween(line.p1.y, line.p2.y, mouse.y)) && this.isBetween(line.p1.x, line.p2.x, mouse.x);
    };
    InkCanvas.prototype.clear = function () {
        this._context.clearRect(0, 0, this._canvas.width, this._canvas.height);
        this._activeStroke = new Stroke();
        this._brush = new HighlightBrush();
    };
    return InkCanvas;
})();
/// <reference path="../InkCanvas.ts" />
var LassoBrush = (function () {
    function LassoBrush() {
    }
    LassoBrush.prototype.draw = function (x, y, inkCanvas) {
        var canvas = inkCanvas._canvas;
        var ctx = inkCanvas._context;
        ctx.globalCompositeOperation = "source-over";
        ctx.fillStyle = '#ff0000';
        ctx.beginPath();
        ctx.moveTo(x, y);
        ctx.arc(x, y, 5, 0, Math.PI * 2, false);
        ctx.fill();
    };
    LassoBrush.prototype.redraw = function (stroke, inkCanvas) {
        inkCanvas.removeStroke();
        for (var i = 0; i < stroke.points.length; i++) {
            this.draw(stroke.points[i].x, stroke.points[i].y, inkCanvas);
        }
    };
    LassoBrush.prototype.focusLine = function (line, inkCanvas) {
        var c = inkCanvas._canvas;
        var ctx = c.getContext("2d");
        ctx.beginPath();
        ctx.fillStyle = '#ff0000';
        ctx.moveTo(line.p1.x, line.p1.y);
        ctx.lineTo(line.p2.x, line.p2.y);
        ctx.lineWidth = 3;
        ctx.stroke();
        ctx.lineWidth = 1;
    };
    LassoBrush.prototype.focusPoint = function (p, inkCanvas) {
        var canvas = inkCanvas._canvas;
        var ctx = inkCanvas._context;
        ctx.globalCompositeOperation = "source-over";
        ctx.fillStyle = '#0000FF';
        ctx.beginPath();
        ctx.moveTo(p.x, p.y);
        ctx.arc(p.x, p.y, 8, 0, Math.PI * 2, false);
        ctx.fill();
    };
    //draw previous on hover
    LassoBrush.prototype.drawline = function (p1, p2, inkCanvas) {
        //  console.log("drawline....");
        var c = inkCanvas._canvas;
        var ctx = inkCanvas._context;
        ctx.lineWidth = 30;
        ctx.fillStyle = '#000000';
        ctx.beginPath();
        ctx.moveTo(p1.x, p1.y);
        ctx.lineTo(p2.x, p2.y);
        ctx.stroke();
    };
    LassoBrush.prototype.drawPrevious = function (stroke, inkCanvas) {
        var _this = this;
        inkCanvas.clear();
        //   console.log("======DRAWPREVLASSO!!");
        stroke.points.forEach(function (p, i) {
            _this.draw(p.x, p.y, inkCanvas);
            if (i == 0) {
                _this.drawline(p, stroke.points[stroke.points.length - 1], inkCanvas);
            }
            else {
                _this.drawline(p, stroke.points[i - 1], inkCanvas);
            }
        });
        //var firstPoint = stroke.points[0];
        //var lastPoint = stroke.points[stroke.points.length - 1];
        //var canvas = inkCanvas._canvas;
        //var ctx = inkCanvas._context;
        //ctx.globalCompositeOperation = "source-over";
        //ctx.beginPath();
        //ctx.lineWidth = 3;
        //ctx.strokeStyle = "rgb(255,70,70)";
        //ctx.setLineDash([5]);
        //ctx.rect(firstPoint.x, firstPoint.y - $(window).scrollTop(), lastPoint.x - firstPoint.x, lastPoint.y - firstPoint.y);
        //ctx.stroke();
    };
    return LassoBrush;
})();
var MarqueeBrush = (function () {
    function MarqueeBrush() {
        this._img = new Image();
        this._img.src = chrome.extension.getURL("assets/brush.png");
    }
    MarqueeBrush.prototype.draw = function (x, y, inkCanvas) {
        inkCanvas.removeStroke();
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
    MarqueeBrush.prototype.redraw = function (stroke, inkCanvas) {
        var firstPoint = stroke.points[0];
        var lastPoint = stroke.points[stroke.points.length - 1];
        this._startX = firstPoint.x;
        this._startY = firstPoint.y;
        this.draw(lastPoint.x, lastPoint.y, inkCanvas);
    };
    MarqueeBrush.prototype.focusLine = function (line, inkCanvas) {
    };
    MarqueeBrush.prototype.focusPoint = function (p, inkCanvas) {
        var canvas = inkCanvas._canvas;
        var ctx = inkCanvas._context;
        ctx.globalCompositeOperation = "source-over";
        ctx.fillStyle = '#0000FF';
        ctx.beginPath();
        ctx.moveTo(p.x, p.y);
        ctx.arc(p.x, p.y, 8, 0, Math.PI * 2, false);
        ctx.fill();
    };
    //draw previous on hover
    MarqueeBrush.prototype.drawPrevious = function (stroke, inkCanvas) {
        console.log("======DRAWPREVMARQUEE");
        console.log(stroke);
        var firstPoint = stroke.points[0];
        var lastPoint = stroke.points[stroke.points.length - 1];
        var canvas = inkCanvas._canvas;
        var ctx = inkCanvas._context;
        ctx.globalCompositeOperation = "source-over";
        ctx.beginPath();
        ctx.lineWidth = 100;
        ctx.strokeStyle = "rgb(0,0,0)";
        ctx.rect(firstPoint.x, firstPoint.y - $(window).scrollTop(), lastPoint.x - firstPoint.x, lastPoint.y - firstPoint.y);
        ctx.stroke();
    };
    return MarqueeBrush;
})();
/// <reference path="StrokeType.ts"/>
var Main = (function () {
    function Main() {
        var _this = this;
        this.body = document.body;
        this.html = document.documentElement;
        this.pointIndex = -1;
        this.selections = new Array();
        this._parsedTextNodes = {};
        this.mouseUp = function (e) {
            console.log("mouseUp");
            if (_this.selectionOnHover) {
                if (_this.is_editing_selection) {
                    _this.inkCanvas.clear();
                    console.log(_this.selectionOnHover);
                    _this.removeHighlight(_this.selectionOnHover);
                    _this.isLineSelected = false;
                    _this.isPointSelected = false;
                    _this.is_editing_selection = false;
                    _this.pointIndex = -1;
                    _this.countX = 0;
                    document.body.removeChild(_this.canvas);
                    var editedSelection = new LassoSelection();
                    var editedStroke = new Stroke();
                    editedStroke.points = _this.selectionOnHover.samplePoints;
                    editedSelection.stroke = editedStroke;
                    editedSelection.end(0, 0);
                    editedSelection.type = StrokeType.Lasso;
                    editedSelection.id = _this.selectionOnHover.id;
                    editedSelection.url = _this.selectionOnHover.url;
                    editedSelection.tags = _this.selectionOnHover.tags;
                    chrome.runtime.sendMessage({ msg: "edit_selection", data: editedSelection });
                    document.body.appendChild(_this.canvas);
                    return;
                }
                else {
                    console.log("======BUBBE");
                    $(_this.bubble).show();
                    $(_this.bubble).css("top", e.clientY - 170 - $(window).scrollTop());
                    $(_this.bubble).css("left", e.clientX - 30);
                    $(_this.bubble).css("border", "8px solid #666");
                    document.styleSheets[0]["insertRule"]('p.noteBubble:before { content: " "; width: 0; height: 0; position: absolute; top: 100px; left: 30px; border: 25px solid #666; border-color: #666 transparent transparent #666; }', 0);
                }
                $(_this.bubble).click(function () {
                });
            }
            else {
            }
            console.log("======================================");
            document.body.removeChild(_this.canvas);
            var isLineSelected = false;
            _this.isSelecting = false;
            _this.isLineSelected = false;
            _this.isPointSelected = false;
            _this.is_editing_selection = false;
            _this.selection.stroke = _this.inkCanvas._activeStroke;
            _this.selection.end(e.clientX, e.clientY);
            console.log(_this.selection.getContent()); //print out content 
            _this.selection.id = Date.now(); //assign contents of the selection 
            _this.selection.type = _this.currentStrokeType;
            _this.selection.url = _this._url;
            _this.selection.tags = $(_this.menuIframe).contents().find("#tagfield").val();
            console.log(_this.selection);
            if (_this.selection.getContent() != "" && _this.selection.getContent() != " ") {
                _this.selections.push(_this.selection); //add selection to selections array 
                //         this.previousSelections.push(this.selection);
                _this.updateSelectedList();
                chrome.runtime.sendMessage({ msg: "store_selection", data: _this.selection });
            }
            _this.inkCanvas.clear();
            //     this.inkCanvas.drawStroke(this.selection.stroke);
            _this.currentStrokeType = StrokeType.Line;
            document.body.appendChild(_this.canvas);
        };
        //mousedown action
        this.mouseDown = function (e) {
            console.log("mouse down");
            _this.selection = new NullSelection();
            //     this.inkCanvas.switchBrush(this.currentStrokeType);
            try {
                document.body.removeChild(_this.canvas);
            }
            catch (e) {
                console.log("no canvas visible." + e);
            }
            if (!_this.checkAtag(e)) {
                _this.isSelecting = true;
                _this._startX = e.clientX;
                _this._startY = e.clientY;
                _this.selection.start(e.clientX, e.clientY);
            }
        };
        this.countX = 0;
        this.getSelectionOnHover = function (e) {
            //    console.log(this.selections);
            for (var i = 0; i < _this.selections.length; i++) {
                if (_this.isAbove(e, _this.selections[i])) {
                    return _this.selections[i];
                }
            }
            //this.selections.forEach((sel, indx) => {
            //    if (this.isAbove(e, sel)) {
            //        console.log("true!!!!");
            //        this.selectionOnHover = sel;
            //        return sel;
            //        //draw red 
            //        //onHover
            //        //clickable-remember variable(selection Current PRev);tu
            //    }
            //});
            return null;
        };
        this.mouseMove = function (e) {
            if (_this.is_editing_selection) {
                var sel = _this.selectionOnHover;
                if (_this.isPointSelected) {
                    if (_this.pointIndex == -1) {
                        _this.pointIndex = _this.findReplacementStroke(sel.samplePoints, _this.pointAbove);
                    }
                    var newPoint = new Point(e.clientX, e.clientY);
                    sel.samplePoints.join();
                    sel.samplePoints.splice(_this.pointIndex, 1, newPoint);
                    sel.samplePoints.join();
                    _this.inkCanvas.clear();
                    _this.inkCanvas.drawPreviousGesture(sel);
                }
                if (_this.isLineSelected) {
                    var index = _this.findInsertionStroke(sel.samplePoints, _this.lineAbove);
                    index++;
                    console.log("mousemove...");
                    console.log(index);
                    var newPoint = new Point(e.clientX, e.clientY);
                    console.log(sel.samplePoints.length);
                    if (_this.countX == 0) {
                        sel.samplePoints.join();
                        sel.samplePoints.splice(index, 0, newPoint);
                        sel.samplePoints.join();
                    }
                    else {
                        sel.samplePoints.join();
                        sel.samplePoints.splice(index, 1, newPoint);
                        sel.samplePoints.join();
                    }
                    console.log(sel.samplePoints.length);
                    _this.inkCanvas.clear();
                    _this.inkCanvas.drawPreviousGesture(sel);
                    console.log("==========DRAW====");
                    _this.countX++;
                }
                _this.selectionOnHover.samplePoints = sel.samplePoints;
            }
            else {
                _this.selectionOnHover = _this.getSelectionOnHover(e);
                if (_this.selectionOnHover) {
                    //         console.log("there is a selection :");
                    //         console.log(this.selectionOnHover);
                    _this.inkCanvas.drawPreviousGesture(_this.selectionOnHover);
                    _this.pointAbove = _this.inkCanvas.editPoint(_this.selectionOnHover.samplePoints, e);
                    if (_this.pointAbove) {
                        console.log("point..... ");
                    }
                    else {
                        _this.lineAbove = _this.inkCanvas.editStrokes(_this.selectionOnHover.samplePoints, e);
                        if (_this.lineAbove) {
                            console.log("line.....");
                        }
                    }
                }
                else if (_this.isSelecting) {
                    _this.inkCanvas.draw(e.clientX, e.clientY);
                    if (_this.currentStrokeType != StrokeType.Lasso && _this.currentStrokeType != StrokeClassifier.getStrokeType(_this.inkCanvas._activeStroke)) {
                        //     console.log("strokeType changed from " + this.currentStrokeType + " to " + StrokeClassifier.getStrokeType(this.inkCanvas._activeStroke));
                        _this.currentStrokeType = StrokeClassifier.getStrokeType(_this.inkCanvas._activeStroke);
                        _this.switchSelection(_this.currentStrokeType);
                        _this.inkCanvas.switchBrush(_this.currentStrokeType);
                    }
                }
                else {
                    _this.inkCanvas.clear();
                    _this.pointAbove = null;
                    _this.lineAbove = null;
                    _this.is_editing_selection = false;
                }
            }
            /*       ///////////////////
                   if (this.isPointSelected) {
                       var sel = this.selectionOnHover;
                       if (sel.type == StrokeType.Lasso) {
                           if (this.pointIndex == -1) {
                               this.pointIndex = this.findReplacementStroke(sel.samplePoints, this.pointAbove);
                           }
                           console.log("----------------------------------");
                           console.log(index);
                           var newPoint = new Point(e.clientX, e.clientY);
                           sel.samplePoints.join();
                           sel.samplePoints.splice(this.pointIndex, 1, newPoint);
                           sel.samplePoints.join();
                           this.inkCanvas.clear();
                           this.inkCanvas.drawPreviousGestureL(sel.samplePoints);
            
                       }
                   }else if (this.isLineSelected) {
                       var sel = this.selectionOnHover;
                       if (sel.type == StrokeType.Lasso) {
                           var index = this.findInsertionStroke(sel.samplePoints, this.lineAbove);
                           index++;
                           console.log("mousemove...");
                           console.log(index);
                           var newPoint = new Point(e.clientX, e.clientY);
                           console.log(sel.samplePoints.length);
                           if (this.countX == 0) {
                               sel.samplePoints.join();
                               sel.samplePoints.splice(index, 0, newPoint);
                               sel.samplePoints.join();
                           } else {
                               sel.samplePoints.join();
                               sel.samplePoints.splice(index, 1, newPoint);
                               sel.samplePoints.join();
                           }
           
                           console.log(sel.samplePoints.length);
                           this.inkCanvas.clear();
                           this.inkCanvas.drawPreviousGestureL(sel.samplePoints);
                           console.log("==========DRAW====");
                           this.countX++;
                       }
                   }
                   else if (this.isSelecting) {
                       this.inkCanvas.draw(e.clientX, e.clientY);
                       if (this.currentStrokeType != StrokeType.Lasso && this.currentStrokeType != StrokeClassifier.getStrokeType(this.inkCanvas._activeStroke)) {
                           console.log("strokeType changed from " + this.currentStrokeType + " to " + StrokeClassifier.getStrokeType(this.inkCanvas._activeStroke));
                           this.currentStrokeType = StrokeClassifier.getStrokeType(this.inkCanvas._activeStroke);
                           this.switchSelection(this.currentStrokeType);
                           this.inkCanvas.switchBrush(this.currentStrokeType);
                       }
                   } else {
                       if (this.is_above_previous) {
                           console.log("check for line intersect...");
                           var line = this.inkCanvas.editStrokes(this.selectionOnHover.samplePoints, new Point(e.clientX, e.clientY));
                           var point = this.inkCanvas.editPoint(this.selectionOnHover.samplePoints, new Point(e.clientX, e.clientY));
                           if (point != null) {
                               console.log("==========POINT==============");
                               console.log(point);
                               this.pointAbove = point;
           
                           }else if (line != null) {
                               console.log("==============LINE==================");
                               console.log(line);
                               this.lineAbove = line;
                               console.log("=============EDITING LINE============");
                               console.log(this.selectionOnHover);
           
                           }
                           this.checkStillOnHover(e);
                       } else {
                           this.showGestureOnHover(e);
                           this.lineAbove = null;
                           this.pointAbove = null;
                       }
                
                   } */
        };
        this.checkNoteBubble = function (e) {
        };
        this.checkAtag = function (e) {
            console.log("checkAtag");
            var hitElem = document.elementFromPoint(e.clientX, e.clientY);
            var res = true;
            console.log(hitElem);
            var el = _this.getSelectionOnHover(e);
            if (_this.pointAbove) {
                _this.isPointSelected = true;
                _this.is_editing_selection = true;
                document.body.appendChild(_this.canvas);
            }
            else if (_this.lineAbove) {
                _this.isLineSelected = true;
                _this.is_editing_selection = true;
                document.body.appendChild(_this.canvas);
            }
            else if (hitElem.nodeName == "A") {
                console.log("atag");
                var link = hitElem.getAttribute("href").toString();
                if (link.indexOf("http") == -1) {
                    link = "http://" + window.location.host + link;
                }
                console.log(link);
                window.open(link, "_self");
            }
            else if (hitElem.nodeName == "TEXTAREA") {
                console.log("textarea");
                _this.bubble_focused = true;
            }
            else {
                document.body.appendChild(_this.canvas);
                res = false;
            }
            return res;
        };
        var body = document.body, html = document.documentElement;
        Main.DOC_WIDTH = Math.max(body.scrollWidth, body.offsetWidth, html.clientWidth, html.scrollWidth, html.offsetWidth);
        Main.DOC_HEIGHT = Math.max(body.scrollHeight, body.offsetHeight, html.clientHeight, html.scrollHeight, html.offsetHeight);
        console.log("Starting Nusys.....!!");
        this.canvas = document.createElement("canvas");
        this.canvas.width = window.innerWidth;
        this.canvas.height = window.innerHeight;
        this.canvas.style.position = "fixed";
        this.canvas.style.top = "0";
        this.canvas.style.left = "0";
        this.canvas.style.zIndex = "998";
        this.bubble_focused = false;
        this.bubble = document.createElement("p");
        this.bubble.innerHTML = "<textarea style=' width: 200px; height: 90px; text-align: center;  -moz-border-radius: 30px;  -webkit-border-radius: 30px; border-radius: 30px; border: none; outline: none; '>";
        $(this.bubble).addClass("noteBubble");
        document.body.appendChild(this.bubble);
        document.styleSheets[0]["insertRule"]('p.noteBubble {position: absolute; width: 200px; height: 100px; text - align: center; line - height: 100px; background: #fff; border: 8px solid #666; -moz-border-radius: 30px;  -webkit-border-radius: 30px; border-radius: 30px; -moz -box-shadow: 2px 2px 4px #888; -webkit-box-shadow: 2px 2px 4px #888; box-shadow: 2px 2px 4px #888; }', 0);
        document.styleSheets[0]["insertRule"]('p.noteBubble:before { content: " "; width: 0; height: 0; position: absolute; top: 100px; left: 30px; border: 25px solid #666; border-color: #666 transparent transparent #666; }', 0);
        document.styleSheets[0]["insertRule"]('p.noteBubble:after { content: " "; width: 0; height: 0; position: absolute; top: 100px; left: 38px; border: 15px solid #fff; border-color: #fff transparent transparent #fff; }', 0);
        $(this.bubble).css("display", "none");
        this.inkCanvas = new InkCanvas(this.canvas);
        this._url = window.location.protocol + "//" + window.location.host + window.location.pathname;
        this.set_message_listener();
        //   this.showPreviousSelections();
        this.body.addEventListener("mousedown", function (e) {
            if (_this.bubble_focused && !_this.isAboveBubble(e)) {
                //set Bubble Speech.... 
                $(_this.bubble).css("display", "none");
                _this.body.appendChild(_this.canvas);
                _this.is_above_previous = false;
                _this.is_editing_selection = false;
                _this.bubble_focused = false;
                _this.mouseDown(e);
            }
        });
    }
    Main.prototype.showPreviousSelections = function () {
        var _this = this;
        chrome.storage.local.get(function (cTedStorage) {
            console.log("STORAGE: ");
            console.info(cTedStorage);
            cTedStorage["selections"].forEach(function (s) {
                if (s.url == _this._url) {
                    //              this.previousSelections.push(s);
                    _this.selections.push(s);
                    _this.updateSelectedList();
                    if (s.type == StrokeType.Marquee) {
                        _this.highlightPrevious(s);
                    }
                    if (s.type == StrokeType.Bracket) {
                        console.log(s);
                        _this.highlightPrevious(s);
                    }
                    if (s.type == StrokeType.Line) {
                        _this.highlightPrevious(s);
                    }
                    if (s.type == StrokeType.Null) {
                        _this.highlightPrevious(s);
                    }
                    if (s.type == StrokeType.Lasso) {
                        console.log("lasso");
                        _this.highlightPrevious(s);
                    }
                }
            });
        });
    };
    Main.prototype.removeHighlight = function (s) {
        s.selectedElements.forEach(function (el) {
            if (el.tagName == "WORD") {
                if (el.wordIndx == -1) {
                    $('WORD').get(el.index)["style"].backgroundColor = "";
                }
                else {
                    var ele = $(el.par).get(el.parIndex).childNodes[el.txtnIndx].childNodes[el.wordIndx];
                    ele["style"].backgroundColor = "";
                }
            }
            else if (el.tagName == "HILIGHT") {
                $(el.par).get(el.parIndex).childNodes[el.txtnIndx]["style"].backgroundColor = "";
            }
            else {
                $(el.tagName).get(el.index).style.backgroundColor = "";
            }
        });
    };
    Main.prototype.highlightPrevious = function (s) {
        var _this = this;
        var parElement;
        var parIndex;
        s.selectedElements.forEach(function (el) {
            if (el.tagName == "WORD") {
                if (el.wordIndx == -1) {
                    $('WORD').get(el.index)["style"].backgroundColor = "yellow";
                }
                else {
                    //    console.log("TAG NAME WORD");
                    //    console.log(el);
                    var txtElement = $(el.par).get(el.parIndex).childNodes[el.txtnIndx];
                    if (!_this._parsedTextNodes.hasOwnProperty(el.par)) {
                        $(txtElement).replaceWith("<words>" + $(txtElement).text().replace(/([^\s]*)/g, "<word>$1</word>") + "</words>");
                        var paridx = {};
                        var txtidx = {};
                        txtidx[el.txtnIndx] = true;
                        paridx[el.parIndex] = txtidx;
                        _this._parsedTextNodes[el.par] = paridx;
                        console.log("change");
                    }
                    else if (!_this._parsedTextNodes[el.par].hasOwnProperty(el.parIndex)) {
                        $(txtElement).replaceWith("<words>" + $(txtElement).text().replace(/([^\s]*)/g, "<word>$1</word>") + "</words>");
                        var txtidx = {};
                        txtidx[el.txtnIndx] = true;
                        _this._parsedTextNodes[el.par][el.parIndex] = txtidx;
                        console.log("change1");
                    }
                    else if (!_this._parsedTextNodes[el.par][el.parIndex].hasOwnProperty(el.txtnIndx)) {
                        console.log(txtElement);
                        _this._parsedTextNodes[el.par][el.parIndex][el.txtnIndx] = true;
                        $(txtElement).replaceWith("<words>" + $(txtElement).text().replace(/([^\s]*)/g, "<word>$1</word>") + "</words>");
                        console.log("change2");
                    }
                    //if (parElement != el.par || parIndex != el.parIndex) {
                    //    $(txtElement).replaceWith("<words>" + $(txtElement).text().replace(/([^\s]*)/g, "<word>$1</worn d>") + "</words>");
                    //    parElement = el.par;
                    //    parIndex = el.parIndex;
                    //}
                    var ele = $(el.par).get(el.parIndex).childNodes[el.txtnIndx].childNodes[el.wordIndx];
                    //console.log(el);
                    //console.log(ele);
                    ele["style"].backgroundColor = "yellow";
                }
            }
            else if (el.tagName == "HILIGHT") {
                //console.log(el);
                //console.log(el.tagName);
                $($(el.par).get(el.parIndex).childNodes[el.txtnIndx]).replaceWith("<hilight>" + $($(el.par).get(el.parIndex).childNodes[el.txtnIndx]).text() + "</hilight>");
                //console.log(el.par);
                $(el.par).get(el.parIndex).childNodes[el.txtnIndx]["style"].backgroundColor = "yellow";
            }
            else {
                if (el.tagName == "IMG") {
                    var label = $("<span class='wow'>Selected</span>");
                    label.css({ position: "absolute", display: "block", background: "yellow", width: "50px", height: "20px", color: "black", "font-size": "12px", padding: "3px 3px", "font-weight": "bold" });
                    $("body").append(label);
                    label.css("top", ($($(el.tagName).get(el.index)).offset().top - 5) + "px");
                    label.css("left", ($($(el.tagName).get(el.index)).offset().left - 5) + "px");
                }
                else {
                    $(el.tagName).get(el.index).style.backgroundColor = "yellow";
                }
            }
        });
    };
    //adds listener to chrome, specifying actions in relation to different incoming messages. 
    Main.prototype.set_message_listener = function () {
        var _this = this;
        chrome.runtime.onMessage.addListener(function (request, sender, sendResponse) {
            console.log("Message: " + request.msg);
            var msg = request.msg;
            switch (msg) {
                case "init":
                    _this.init_menu(request.data);
                    sendResponse(true);
                    break;
                case "show_menu":
                    $(_this.menuIframe).css("display", "block");
                    if (_this.is_active) {
                        document.body.appendChild(_this.canvas);
                    }
                    ;
                    break;
                case "hide_menu":
                    if (document.body.contains(_this.canvas))
                        document.body.removeChild(_this.canvas);
                    $(_this.menuIframe).css("display", "none");
                    break;
                case "enable_selection":
                    _this.toggleEnabled(true);
                    break;
                case "disable_selection":
                    _this.toggleEnabled(false);
                    break;
                case "set_selections":
                    break;
                case "tags_changed":
                    console.log("tags_changed");
                    $(_this.menuIframe).contents().find("#tagfield").val(request.data);
                    break;
            }
        });
    };
    //initializes the iframe menu
    Main.prototype.init_menu = function (menuHtml) {
        var _this = this;
        console.log("init!");
        this.menuIframe = $("<iframe frameborder=0>")[0];
        document.body.appendChild(this.menuIframe);
        this.menu = $(menuHtml)[0];
        $(this.menuIframe).css({ position: "fixed", top: "1px", right: "1px", width: "410px", height: "106px", "z-index": 1001 });
        $(this.menuIframe).contents().find('html').html(this.menu.outerHTML);
        $(this.menuIframe).css("display", "none"); //initially set menu to display none.
        $(this.menuIframe).contents().find("#btnExport").click(function (ev) {
            chrome.runtime.sendMessage({ msg: "export" });
        });
        $(this.menuIframe).contents().find("#btnLineSelect").click(function (ev) {
            console.log("btnLineSelect==========================");
        });
        $(this.menuIframe).contents().find("#btnBlockSelect").click(function (ev) {
            console.log("btnBlockSelect========================");
        });
        $(this.menuIframe).contents().find("#tagfield").change(function () {
            chrome.runtime.sendMessage({ msg: "tags_changed", data: $(_this.menuIframe).contents().find("#tagfield").val() });
            _this._tag = $(_this.menuIframe).contents().find("#tagfield").val();
        });
        $(this.menuIframe).contents().find("#btnViewAll").click(function () {
            chrome.runtime.sendMessage({ msg: "view_all" });
        });
        $(this.menuIframe).contents().find("#toggle").change(function () {
            chrome.runtime.sendMessage({ msg: "set_active", data: $(_this.menuIframe).contents().find("#toggle").prop("checked") });
            _this.toggleEnabled($(_this.menuIframe).contents().find("#toggle").prop("checked"));
        });
        $(this.menuIframe).contents().find("#btnExpand").click(function (ev) {
            console.log("expand");
            var list = $(_this.menuIframe).contents().find("#selected_list");
            if ($(ev.target).hasClass("active")) {
                $(ev.target).removeClass("active");
                $(list).removeClass("open");
                $(_this.menuIframe).height(106);
            }
            else {
                $(ev.target).addClass("active");
                $(list).addClass("open");
                $(_this.menuIframe).height(500);
            }
        });
        chrome.runtime.sendMessage({ msg: "query_active" }, function (isActive) {
            $(_this.menuIframe).contents().find("#toggle").prop("checked", isActive);
        });
    };
    Main.prototype.sendResponse = function (bool) {
    };
    Main.prototype.makeNewLasso = function () {
    };
    Main.prototype.updateSelectedList = function () {
        var _this = this;
        var list = $(this.menuIframe).contents().find("#selected_list");
        list.empty();
        var count = 0;
        this.selections.forEach(function (s) {
            console.info(s);
            var item = document.createElement("div");
            item.setAttribute("class", "selected_list_item");
            var close_btn = document.createElement("button");
            close_btn.setAttribute("class", "btn_close_item");
            $(close_btn).click(function () {
                console.log("remove");
                var indx = _this.selections.indexOf(s);
                console.log(indx);
                console.log(_this.selections[indx]);
                _this.removeHighlight(_this.selections[indx]);
                chrome.runtime.sendMessage({ msg: "remove_selection", data: _this.selections[indx]["id"] });
                _this.selections.splice(_this.selections.indexOf(s), 1);
                //        this.previousSelections.splice(this.previousSelections.indexOf(s), 1);
                close_btn.parentElement.remove();
            });
            item["innerHTML"] = s["_content"];
            $(item).prepend(close_btn);
            count++;
            list.append(item);
        });
    };
    Main.prototype.findReplacementStroke = function (points, p) {
        console.log(p);
        console.log(points);
        var size = points.length;
        for (var i = 0; i < size; i++) {
            if (points[i].x == p.x && points[i].y == p.y)
                return i;
        }
        return -1;
    };
    Main.prototype.findInsertionStroke = function (points, line) {
        var size = points.length;
        for (var i = 0; i < size; i++) {
            if (points[i].x == line.p1.x && points[i].y == line.p1.y)
                return i;
        }
        //points.forEach((p, i) => {
        //    console.log(p);
        //    console.log(line);
        //    console.log(p.x == line.p1.x);
        //    if (p.x == line.p1.x && p.y == line.p1.y) {
        //        return i;
        //    }
        //});
        console.log("================!!!");
        return -1;
    };
    Main.prototype.checkStillOnHover = function (e) {
        if (!this.isAbove(e, this.selectionOnHover)) {
            this.inkCanvas.clear();
            this.is_above_previous = false;
        }
    };
    Main.prototype.showGestureOnHover = function (e) {
        var _this = this;
        this.selections.forEach(function (sel, indx) {
            if (_this.isAbove(e, sel)) {
                console.log("isAbove!!!! " + sel);
                _this.selectionOnHover = sel;
                _this.is_above_previous = true;
                _this.inkCanvas.drawPreviousGesture(sel);
            }
        });
    };
    // checks if current mouse is above previous, must consider scrollTop
    Main.prototype.isAbove = function (e, sel) {
        var stroke = new Stroke();
        stroke.points = sel.samplePoints;
        return this.isPointBound(new Point(e.clientX, e.clientY), stroke);
    };
    Main.prototype.sampleLines = function (stroke) {
        var sampleStroke = stroke.points;
        var lines = [];
        for (var i = 1; i < sampleStroke.length; i++) {
            lines.push(new Line(sampleStroke[i - 1], sampleStroke[i]));
        }
        lines.push(new Line(sampleStroke[sampleStroke.length - 1], sampleStroke[0]));
        return lines;
    };
    ///directly from lasso
    Main.prototype.isPointBound = function (p, s) {
        var lines = this.sampleLines(s);
        //  console.log("======isPointBound ");
        //  console.log(p);
        var xPoints = [];
        for (var i = 0; i < lines.length; i++) {
            var l = lines[i];
            if (p.y <= Math.max(l.p1.y, l.p2.y) && p.y >= Math.min(l.p1.y, l.p2.y)) {
                var x = (l.C - l.B * p.y) / l.A;
                xPoints.push(x);
            }
        }
        //  console.log(xPoints);
        if (xPoints.length == 0)
            return false;
        xPoints.sort(function (a, b) { return a - b; });
        var res = false;
        //for compromise
        for (var i = 0; i < xPoints.length; i++) {
            var xval = xPoints[i];
            if (i == 0)
                xval -= 30;
            if (i == xPoints.length - 1)
                xval += 30;
            if (p.x < xval)
                return res;
            res = !res;
        }
        return false;
    };
    Main.prototype.isAboveBubble = function (e) {
        return (e.clientX > this.bubble.offsetLeft && e.clientX < this.bubble.offsetLeft + 200) && (e.clientY > this.bubble.offsetTop - $(window).scrollTop() && e.clientY < this.bubble.offsetTop + 200 - $(window).scrollTop());
        //return bool
    };
    Main.prototype.switchSelection = function (strokeType) {
        console.log("Iselection switched to : " + strokeType);
        switch (strokeType) {
            //////STROKE TYPE CHANGE
            case StrokeType.Marquee:
                this.selection = new MarqueeSelection();
                break;
            case StrokeType.Bracket:
                this.selection = new BracketSelection();
                break;
            case StrokeType.Line:
                this.selection = new LineSelection();
                break;
            case StrokeType.Lasso:
                console.log("============================!!!!!!!!!!!!!!========================");
                this.selection = new LassoSelection();
                break;
        }
        this.selection.start(this._startX, this._startY);
    };
    Main.prototype.showBubble = function () {
        $(this.bubble).show();
    };
    Main.prototype.isAboveLine = function (points) {
        return false;
    };
    Main.prototype.toggleEnabled = function (flag) {
        console.log("toggle state changed");
        $(this.menuIframe).contents().find("#toggle").prop("checked", flag);
        //called to add or remove canvas when toggle has been changed
        this.is_active = flag;
        if (this.is_active) {
            try {
                document.body.appendChild(this.canvas);
                console.log("added canvas");
            }
            catch (ex) {
                console.log("could't add canvas");
            }
            this.currentStrokeType = StrokeType.Null;
            this.canvas.addEventListener("mousedown", this.mouseDown);
            this.canvas.addEventListener("mouseup", this.mouseUp);
            this.canvas.addEventListener("mousemove", this.mouseMove);
        }
        else {
            try {
                document.body.removeChild(this.canvas);
            }
            catch (e) {
                console.log("no canvas visible." + e);
            }
        }
    };
    return Main;
})();
$(document).ready(function () {
    console.log("REQDT");
    var main = new Main();
});
var AbstractSelection = (function () {
    function AbstractSelection() {
        this.selectedElements = new Array();
    }
    AbstractSelection.prototype.start = function (x, y) { };
    AbstractSelection.prototype.end = function (x, y) { };
    AbstractSelection.prototype.analyzeContent = function () { };
    AbstractSelection.prototype.getContent = function () {
        return null;
    };
    return AbstractSelection;
})();
var BracketSelection = (function (_super) {
    __extends(BracketSelection, _super);
    function BracketSelection() {
        _super.call(this);
        this._startX = 0;
        this._startY = 0;
        this._endX = 0;
        this._endY = 0;
        this._content = "";
        console.log("BRACKET SELECTION");
    }
    BracketSelection.prototype.start = function (x, y) {
        this._startX = x;
        this._startY = y;
        console.log("bracket start" + x + ":" + y);
    };
    BracketSelection.prototype.end = function (x, y) {
        this._endX = x;
        this._endY = y;
        this.analyzeContent();
        console.log("bracket end" + x + ":" + y);
    };
    BracketSelection.prototype.getContent = function () {
        return this._content;
    };
    BracketSelection.prototype.analyzeContent = function () {
        var _this = this;
        console.log("bracket starts analyzing content....");
        var stroke = this.stroke;
        var selectionBB = stroke.getBoundingRect();
        selectionBB.w = Main.DOC_WIDTH - selectionBB.x; // TODO: fix this magic number
        var samplingRate = 50;
        var numSamples = 0;
        var totalScore = 0;
        var hitCounter = new collections.Dictionary(function (elem) { return elem.outerHTML.toString(); });
        var elList = [];
        var scoreList = [];
        for (var x = selectionBB.x; x < selectionBB.x + selectionBB.w; x += samplingRate) {
            for (var y = selectionBB.y; y < selectionBB.y + selectionBB.h; y += samplingRate) {
                var hitElem = document.elementFromPoint(x, y);
                if ($(hitElem).height() > selectionBB.h + selectionBB.h / 2.0) {
                    continue;
                }
                else {
                }
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
        console.log(hitCounter);
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
        console.log("initial candidates");
        console.log(candidates);
        var std = Statistics.getStandardDeviation(candidates, precision);
        var maxDev = maxScore - 2 * std;
        var finalCandiates = [];
        hitCounter.forEach(function (k, v) {
            if (v >= maxDev && v <= maxScore) {
                finalCandiates.push(k);
            }
        });
        console.log("initial candidates");
        console.log(finalCandiates);
        //finalCandiates = [finalCandiates[0]];
        /*
        var selectedElements = finalCandiates.filter((candidate) => {
            
            if ($(candidate).prop('style').float == "left") {
                console.log("found float");
                return true;
            }

            return false;
        });
        
        if (selectedElements.length == 0) {
            selectedElements = finalCandiates.filter((candidate) => {
           
                if ($(candidate).offset().left - selectionBB.x < 100) {
                    return true;
                }

                return false;
            });
        }
        */
        finalCandiates.concat().forEach(function (c) {
            var maxDelta = 120;
            var largerParents = [];
            var parents = $(c).parents();
            for (var i = 0; i < parents.length; i++) {
                var parent = $(parents[i]);
                if (parent.width() - $(c).width() < maxDelta && parent.height() - $(c).height() < maxDelta) {
                    var index = finalCandiates.indexOf(c);
                    if (index > 0) {
                        finalCandiates.splice(index, 1);
                        largerParents.push(parent[0]);
                    }
                }
            }
            if (largerParents.length > 0)
                finalCandiates.push(largerParents.pop());
        });
        console.log("initial candidates with parents");
        console.log(finalCandiates);
        var selectedElements = finalCandiates.filter(function (candidate) {
            if ($(candidate).offset().left - selectionBB.x < 100) {
                return true;
            }
            return false;
        });
        console.log("selected elements");
        console.log(selectedElements);
        //    this._clientRects = new Array<ClientRect>();
        var result = "";
        selectedElements.forEach(function (el) {
            //       console.log(
            $(el).find("img")["andSelf"]().each(function (i, e) {
                $(e).attr("src", e.src);
                $(e).removeAttr("srcset");
            });
            var range = document.createRange();
            range.selectNodeContents(el);
            var rects = range.getClientRects();
            //       this._clientRects = this._clientRects.concat.apply([], rects);
            var index = $(el.tagName).index(el);
            _this.selectedElements.push({ type: "bracket", tagName: el.tagName, index: index });
            console.log("selected element: " + el.tagName + "." + index);
            result += el.outerHTML;
            console.log(el);
            $(el).css("background-color", "yellow");
        });
        this._content = result;
    };
    return BracketSelection;
})(AbstractSelection);
var LassoSelection = (function (_super) {
    __extends(LassoSelection, _super);
    function LassoSelection() {
        _super.apply(this, arguments);
        this._parentList = new Array();
        this._minX = 10000;
        this._minY = 10000;
        this._maxX = 0;
        this._maxY = 0;
        this._content = "";
    }
    LassoSelection.prototype.start = function (x, y) {
    };
    LassoSelection.prototype.end = function (x, y) {
        this.samplePoints = this.stroke.sampleStroke().points;
        this._sampleLines = this.sampleLines(this.samplePoints);
        this.analyzeContent();
    };
    LassoSelection.prototype.analyzeContent = function () {
        this.makeInitialParentList();
        console.info(this._parentList);
        this.findCommonParent();
        console.log("=====rmChildforLasso");
        console.info(this._parentList);
        var parent = this._parentList[0].cloneNode(true);
        if (parent.nodeName == "html") {
            console.log("=========================PARENT WRONG ================");
            return;
        }
        this.rmChildNodes(parent, this._parentList[0]);
        this._content = parent.innerHTML;
    };
    LassoSelection.prototype.makeInitialParentList = function () {
        var _this = this;
        var element;
        this.stroke.points.forEach(function (p) {
            if (element != document.elementFromPoint(p.x, p.y)) {
                element = document.elementFromPoint(p.x, p.y);
                _this._parentList.push(element);
            }
        });
    };
    LassoSelection.prototype.sampleLines = function (points) {
        var sampleStroke = points;
        var lines = [];
        for (var i = 1; i < sampleStroke.length; i++) {
            lines.push(new Line(sampleStroke[i - 1], sampleStroke[i]));
        }
        lines.push(new Line(sampleStroke[sampleStroke.length - 1], sampleStroke[0]));
        return lines;
    };
    //    l1 is rectlines --> horizontal or vertical 
    //islineintersect(l1, l2: Line): boolean {
    //    console.log("isLineInstersect");
    //    console.log(l1);
    //     console.log(l2);
    //     var det = l1.A * l2.B - l2.A * l1.B;
    //   //  console.log(det);
    //     if (det == 0)
    //         return false;
    //     var x = (l2.B * l1.C - l1.B * l2.C) / det;
    //     var y = (l1.A * l2.C - l2.A * l1.C) / det;
    //     console.log(x + ": " +  y);
    //     var pt = new Point(x, y);
    //     return l1.hasPoint(pt) && l2.hasPoint(pt);
    //}
    LassoSelection.prototype.isLineIntersect = function (l1, l2) {
        if (l1.p1.x == l1.p2.x) {
            //vertical line 
            if (l1.p1.x <= Math.max(l2.p1.x, l2.p2.x) && l1.p1.x >= Math.min(l2.p1.x, l2.p2.x)) {
                var y = (l2.C - l2.A * l1.p1.x) / l2.B;
                return y <= Math.max(l1.p1.y, l1.p2.y) && y >= Math.min(l1.p1.y, l1.p2.y);
            }
        }
        else {
            if (l1.p1.y <= Math.max(l2.p1.y, l2.p2.y) && l1.p1.y >= Math.min(l2.p1.y, l2.p2.y)) {
                var x = (l2.C - l2.B * l1.p1.y) / l2.A;
                return x <= Math.max(l1.p1.x, l1.p2.x) && x >= Math.min(l1.p1.x, l1.p2.x);
            }
        }
        return false;
    };
    LassoSelection.prototype.intersectWith = function (el) {
        //       console.log("intersectWith.... ");
        //      console.log(el);
        if (!el)
            return 0;
        if (this.isTextElement(el)) {
            //        console.log(el);
            var range = document.createRange();
            range.selectNodeContents(el);
            var rects = range.getClientRects();
            if (rects.length == 0) {
                return 0;
            }
            //      console.log("rects: ");
            for (var i = 0; i < rects.length; i++) {
                //        console.log(rects[i]);
                var rect = rects[i];
                if (this.isRectIntersect(new Rectangle(rect.left, rect.top, rect.width, rect.height))) {
                    //              console.log("isIntersect!");
                    return 1;
                }
            }
            if (this.isPointBound(new Point(rects[0].left, rects[0].top))) {
                //        console.log("=====BOUND");
                return 2;
            }
            return 0;
        }
        else if (!this.isCommentElement(el)) {
            var rangeY = document.createRange();
            rangeY.selectNodeContents(el);
            var realDim = this.getRealHeightWidth(rangeY.getClientRects());
            var realHeight = realDim[0];
            var realWidth = realDim[1];
            var minX = realDim[2];
            var minY = realDim[3];
            /////works weird for Wikipedia. 
            if (minX > 100000 || minY > 100000) {
                console.log("WTF!!!!>>????");
                console.log(el);
                return 0;
            }
            //          console.log(el);
            if (this.isRectIntersect(new Rectangle(minX, minY, realWidth, realHeight))) {
                //            console.log("======1");
                return 1;
            }
            if (this.isPointBound(new Point(minX, minY))) {
                //          console.log("======2");
                return 2;
            }
            //        console.log("======0");
            return 0;
        }
    };
    LassoSelection.prototype.isPointBound = function (p) {
        //     console.log("======isPointBound ");
        //    console.log(p);
        var xPoints = [];
        for (var i = 0; i < this._sampleLines.length; i++) {
            var l = this._sampleLines[i];
            if (p.y <= Math.max(l.p1.y, l.p2.y) && p.y >= Math.min(l.p1.y, l.p2.y)) {
                var x = (l.C - l.B * p.y) / l.A;
                xPoints.push(x);
            }
        }
        //    console.log(xPoints);
        if (xPoints.length == 0)
            return false;
        xPoints.sort(function (a, b) { return a - b; });
        var res = false;
        //  console.log(xPoints);
        for (var i = 0; i < xPoints.length; i++) {
            if (p.x < xPoints[i])
                return res;
            res = !res;
        }
        return false;
    };
    LassoSelection.prototype.isCommentElement = function (el) {
        return el.nodeName == "#comment";
    };
    //move to util
    LassoSelection.prototype.getRealHeightWidth = function (rectsList) {
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
    LassoSelection.prototype.isRectIntersect = function (rect) {
        //      console.log(rect);
        var lines = rect.getLines();
        //lines.forEach(l => {
        //    this._sampleLines.forEach(m => {
        //        if (this.isLineIntersect(l, m)) {
        //            console.log("true...");
        //            return true;
        //        }
        //    });
        //});
        for (var i = 0; i < lines.length; i++) {
            for (var j = 0; j < this._sampleLines.length; j++) {
                if (this.isLineIntersect(lines[i], this._sampleLines[j])) {
                    ////                  console.log("==================TRUE=============");
                    return true;
                }
            }
        }
        //      console.log("===================NO INTERSECTING LINE with RECT");
        //      console.log(rect);
        //      console.log(rect.hasPoint(this.stroke.points[0]));
        return rect.hasPoint(this.stroke.points[0]);
    };
    LassoSelection.prototype.isTextElement = function (el) {
        return (el.nodeName == "#text");
    };
    LassoSelection.prototype.rmChildNodes = function (el, trueEl) {
        //      console.log("removeChildNodes for... ");
        //      console.log(trueEl);
        var removed = [];
        var realNList = [];
        var resList = [];
        var indexList = [];
        //iterate through childNodes and add to list(removed).
        for (var i = 0; i < el.childNodes.length; i++) {
            var res = this.intersectWith(trueEl.childNodes[i]);
            //        console.log(trueEl.childNodes[i]);
            //        console.log(res);
            if (res == 0) {
                removed.push(el.childNodes[i]);
            }
            else {
                realNList.push(trueEl.childNodes[i]);
                resList.push(res);
                indexList.push(i);
            }
        }
        //remove not intersecting elements; 
        for (var i = 0; i < removed.length; i++) {
            el.removeChild(removed[i]);
        }
        //       console.log(realNList);
        //       console.log(resList);
        for (var i = 0; i < el.childNodes.length; i++) {
            if (resList[i] != 2) {
                if (this.isTextElement(el.childNodes[i])) {
                    var index = indexList[i];
                    $(trueEl.childNodes[index]).replaceWith("<words>" + $(trueEl.childNodes[index]).text().replace(/([^\s]*)/g, "<word>$1</word>") + "</words>");
                    var result = "";
                    for (var j = 0; j < trueEl.childNodes[index].childNodes.length; j++) {
                        //                console.log(trueEl.childNodes[index].childNodes[j]);
                        if (this.intersectWith(trueEl.childNodes[index].childNodes[j]) > 0) {
                            //                    console.log("included!!!!");
                            if (trueEl.childNodes[index].childNodes[j].style) {
                                trueEl.childNodes[index].childNodes[j].style.backgroundColor = "yellow";
                                this.addToHighLights(trueEl.childNodes[index].childNodes[j], indexList[i], j);
                            }
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
                ////              console.log("BOUNDEDDDD=====");
                //              console.log(trueEl.childNodes[indexList[i]]);
                var startIndex = Array.prototype.indexOf.call(trueEl.childNodes, trueEl.childNodes[i]);
                var foundElement = $(trueEl.childNodes[indexList[i]]).find("img");
                if (foundElement.length > 0) {
                    var label = $("<span class='wow'>Selected</span>");
                    label.css({ position: "absolute", display: "block", background: "yellow", width: "50px", height: "20px", color: "black", "font-size": "12px", padding: "3px 3px", "font-weight": "bold" });
                    $("body").append(label);
                    label.css("top", ($(foundElement).offset().top - 5) + "px");
                    label.css("left", ($(foundElement).offset().left - 5) + "px");
                }
                if (trueEl.childNodes[indexList[i]].childNodes.length == 0) {
                    //                console.log("-----------TEXT?-------");
                    //                console.log($(trueEl.childNodes[indexList[i]]));
                    $(trueEl.childNodes[indexList[i]]).replaceWith("<hilight>" + $(realNList[i]).text() + "</hilight>");
                }
                $(realNList[i]).css("background-color", "yellow");
                trueEl.childNodes[indexList[i]].style.backgroundColor = "yellow";
                this.addToHighLights(trueEl.childNodes[indexList[i]], indexList[i], -1);
            }
        }
    };
    LassoSelection.prototype.addToHighLights = function (el, txtindx, wordindx) {
        var index = $(el.tagName).index(el);
        var obj = { type: "lasso", tagName: el.tagName, index: index };
        if (el.tagName == "WORD" || el.tagName == "HILIGHT") {
            var par = el.attributes[0]["ownerElement"].parentElement;
            if (el.tagName == "WORD") {
                var startIndex = Array.prototype.indexOf.call(el.parentElement.childNodes, el);
                par = par.parentElement;
                obj["wordIndx"] = wordindx;
            }
            var parIndex = $(par.tagName).index(par);
            obj["par"] = par.tagName;
            obj["parIndex"] = parIndex;
            obj["txtnIndx"] = txtindx;
            obj["val"] = el;
        }
        this.selectedElements.push(obj);
    };
    LassoSelection.prototype.findCommonParent = function () {
        if (this._parentList.length != 1) {
            for (var i = 1; i < this._parentList.length; i++) {
                var currAn = this.commonParent(this._parentList[0], this._parentList[i]);
                this._parentList[0] = currAn;
            }
        }
    };
    LassoSelection.prototype.commonParent = function (node1, node2) {
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
    LassoSelection.prototype.parents = function (node) {
        var nodes = [node];
        while (node != null) {
            node = node.parentNode;
            nodes.unshift(node);
        }
        return nodes;
    };
    LassoSelection.prototype.getContent = function () {
        return this._content;
    };
    LassoSelection.prototype.changeMinMax = function (point) {
        var x = point.x;
        var y = point.y;
        if (x < this._minX)
            this._minX = x;
        if (x > this._maxX)
            this._maxX = x;
        if (y > this._maxY)
            this._maxY = y;
        if (y < this._minY)
            this._minY = y;
    };
    return LassoSelection;
})(AbstractSelection);
var LineSelection = (function (_super) {
    __extends(LineSelection, _super);
    function LineSelection() {
        _super.call(this);
        this._startX = 0;
        this._startY = 0;
        this._endX = 0;
        this._endY = 0;
        this._content = "";
        this._parentList = new Array();
        console.log("Line SELECTION");
    }
    LineSelection.prototype.start = function (x, y) {
        this._startX = x;
        this._startY = y;
        console.log("line start" + x + ":" + y);
    };
    LineSelection.prototype.end = function (x, y) {
        this._endX = x;
        this._endY = y;
        this.analyzeContent();
        console.log("line end" + x + ":" + y);
    };
    LineSelection.prototype.getContent = function () {
        return this._content;
    };
    LineSelection.prototype.analyzeContent = function () {
        this.findParentList();
        console.log("marquee start analyzing content....");
        this.findCommonParent();
        var parent = this._parentList[0].cloneNode(true);
        this.rmChildNodes(parent, this._parentList[0]);
        console.log(this._parentList[0]);
        this._content = parent.innerHTML;
    };
    LineSelection.prototype.rmChildNodes = function (el, trueEl) {
        var removed = [];
        var realNList = [];
        var indexList = [];
        //iterate through childNodes and add to list(removed).
        for (var i = 0; i < el.childNodes.length; i++) {
            if (!this.intersectWith(trueEl.childNodes[i])) {
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
                    $(trueEl.childNodes[index]).replaceWith("<words>" + $(trueEl.childNodes[index]).text().replace(/([^\s]*)/g, "<word>$1</word>") + "</words>");
                    var result = "";
                    for (var j = 0; j < trueEl.childNodes[index].childNodes.length; j++) {
                        if (this.intersectWith(trueEl.childNodes[index].childNodes[j])) {
                            if (trueEl.childNodes[index].childNodes[j].style) {
                                trueEl.childNodes[index].childNodes[j].style.backgroundColor = "yellow";
                                console.log(trueEl.childNodes[index]);
                                this.addToHighLights(trueEl.childNodes[index].childNodes[j], indexList[i], j);
                            }
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
                console.log("BOUNDEDDDD=====");
                console.log(trueEl.childNodes[indexList[i]]);
                var startIndex = Array.prototype.indexOf.call(trueEl.childNodes, trueEl.childNodes[i]);
                var foundElement = $(trueEl.childNodes[indexList[i]]).find("img");
                if (foundElement.length > 0) {
                    var label = $("<span class='wow'>Selected</span>");
                    label.css({ position: "absolute", display: "block", background: "yellow", width: "50px", height: "20px", color: "black", "font-size": "12px", padding: "3px 3px", "font-weight": "bold" });
                    $("body").append(label);
                    label.css("top", ($(foundElement).offset().top - 5) + "px");
                    label.css("left", ($(foundElement).offset().left - 5) + "px");
                }
                if (trueEl.childNodes[indexList[i]].childNodes.length == 0) {
                    console.log("-----------TEXT?-------");
                    console.log($(trueEl.childNodes[indexList[i]]));
                    $(trueEl.childNodes[indexList[i]]).replaceWith("<hilight>" + $(realNList[i]).text() + "</hilight>");
                }
                $(realNList[i]).css("background-color", "yellow");
                trueEl.childNodes[indexList[i]].style.backgroundColor = "yellow";
                console.log(startIndex);
                this.addToHighLights(trueEl.childNodes[indexList[i]], indexList[i], -1);
            }
        }
    };
    LineSelection.prototype.addToHighLights = function (el, txtindx, wordindx) {
        console.log("ADD TO HIGHLIGHTS====================");
        console.info(el.tagName);
        console.log(el.attributes);
        var index = $(el.tagName).index(el);
        console.log(index);
        var obj = { type: "line", tagName: el.tagName, index: index };
        if (el.tagName == "WORD" || el.tagName == "HILIGHT") {
            console.log("-------------DIFFICULT--------------");
            console.log(el.attributes);
            var par = el.attributes[0]["ownerElement"].parentElement;
            if (el.tagName == "WORD") {
                var startIndex = Array.prototype.indexOf.call(el.parentElement.childNodes, el);
                par = par.parentElement;
                obj["wordIndx"] = wordindx;
                console.log(par);
            }
            var parIndex = $(par.tagName).index(par);
            obj["par"] = par.tagName;
            obj["parIndex"] = parIndex;
            obj["txtnIndx"] = txtindx;
            obj["val"] = el;
            console.log(el.attributes[0]["ownerElement"].parentElement);
            console.log(obj);
        }
        this.selectedElements.push(obj);
        console.log(this.selectedElements);
    };
    LineSelection.prototype.intersectWith = function (el) {
        //checks if element is intersecting with selection range 
        if (!el) {
            return false;
        }
        ;
        var bx1 = this._startX;
        var bx2 = this._endX;
        var by1 = this._startY;
        var by2 = this._endY;
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
    LineSelection.prototype.bound = function (myEl, el) {
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
                if (!(ax1 >= this._startX &&
                    ax2 <= this._endX &&
                    ay1 >= this._startY &&
                    ay2 <= this._endY)) {
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
            if (rectX["left"] >= this._startX &&
                rectX["left"] + realWidth <= this._endX &&
                rectX["top"] >= this._startY &&
                rectX["top"] + realHeight <= this._endY) {
                //      this.setTextStyle(myEl, el);
                return true;
            }
            return false;
        }
    };
    LineSelection.prototype.getRealHeightWidth = function (rectsList) {
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
    LineSelection.prototype.findCommonParent = function () {
        if (this._parentList.length != 1) {
            for (var i = 1; i < this._parentList.length; i++) {
                var currAn = this.commonParent(this._parentList[0], this._parentList[i]);
                this._parentList[0] = currAn;
            }
        }
    };
    LineSelection.prototype.commonParent = function (node1, node2) {
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
    LineSelection.prototype.parents = function (node) {
        var nodes = [node];
        while (node != null) {
            node = node.parentNode;
            nodes.unshift(node);
        }
        return nodes;
    };
    LineSelection.prototype.findParentList = function () {
        this._parentList = [];
        var el = document.elementFromPoint(this._startX, this._startY);
        //      this._parentList.push(el);
        if (el != null)
            this.findNextElement(el);
        console.info(this._parentList);
    };
    LineSelection.prototype.findNextElement = function (el) {
        var rect = el.getBoundingClientRect();
        var nextX = this._endX - (rect.left + rect.width);
        var nextY = this._endY - (rect.top + rect.height);
        if (this._parentList.indexOf(el) > -1)
            return;
        this._parentList.push(el);
        console.info(el);
        if (el.nodeName == "HTML")
            return;
        if (nextX > 0) {
            console.log("more on the X AXIS");
            console.info(document.elementFromPoint(this._endX - nextX + 1, this._startY));
            this.findNextElement(document.elementFromPoint(this._endX - nextX + 1, this._startY));
        }
    };
    return LineSelection;
})(AbstractSelection);
var MarqueeSelection = (function (_super) {
    __extends(MarqueeSelection, _super);
    function MarqueeSelection() {
        _super.call(this);
        this._startX = 0;
        this._startY = 0;
        this._endX = 0;
        this._endY = 0;
        this._nextX = 0;
        this._nextY = 0;
        this._content = "";
        this._parentList = new Array();
        console.log("MARQUEE SELECTION");
    }
    MarqueeSelection.prototype.start = function (x, y) {
        this._startX = x;
        this._startY = y;
        console.log("marquee start" + x + ":" + y);
    };
    MarqueeSelection.prototype.end = function (x, y) {
        this._endX = x;
        this._endY = y;
        if (this._startX > this._endX) {
            var temp = this._startX;
            this._startX = this._endX;
            this._endX = temp;
        }
        if (this._startY > this._endY) {
            var temp = this._startY;
            this._startY = this._endY;
            this._endY = temp;
        }
        this.analyzeContent();
        console.log(this._startX + "START" + this._startY);
        console.log("marquee end" + x + ":" + y);
        this.samplePoints = [new Point(this._startX, this._startY), new Point(this._endX, this._startY), new Point(this._endX, this._endY), new Point(this._startX, this._endY)];
    };
    MarqueeSelection.prototype.getContent = function () {
        return this._content;
    };
    MarqueeSelection.prototype.analyzeContent = function () {
        this.findParentList();
        console.log("marquee start analyzing content....");
        this.findCommonParent();
        var parent = this._parentList[0].cloneNode(true);
        this.rmChildNodes(parent, this._parentList[0]);
        console.log(this._parentList[0]);
        $(parent).find("img")["andSelf"]().each(function (i, e) {
            console.log(e.src);
            $(e).attr("src", e.src);
            $(e).removeAttr("srcset");
        });
        this._content = parent.innerHTML;
    };
    MarqueeSelection.prototype.rmChildNodes = function (el, trueEl) {
        var removed = [];
        var realNList = [];
        var indexList = [];
        //iterate through childNodes and add to list(removed).
        for (var i = 0; i < el.childNodes.length; i++) {
            if (!this.intersectWith(trueEl.childNodes[i])) {
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
                    $(trueEl.childNodes[index]).replaceWith("<words>" + $(trueEl.childNodes[index]).text().replace(/([^\s]*)/g, "<word>$1</word>") + "</words>");
                    var result = "";
                    for (var j = 0; j < trueEl.childNodes[index].childNodes.length; j++) {
                        if (this.intersectWith(trueEl.childNodes[index].childNodes[j])) {
                            if (trueEl.childNodes[index].childNodes[j].style) {
                                trueEl.childNodes[index].childNodes[j].style.backgroundColor = "yellow";
                                //      console.log(trueEl.childNodes[index]);
                                this.addToHighLights(trueEl.childNodes[index].childNodes[j], indexList[i], j);
                            }
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
                //    console.log("BOUNDEDDDD=====");
                //    console.log(trueEl.childNodes[indexList[i]]);
                var startIndex = Array.prototype.indexOf.call(trueEl.childNodes, trueEl.childNodes[i]);
                var foundElement = $(trueEl.childNodes[indexList[i]]).find("img");
                if (foundElement.length > 0) {
                    var label = $("<span class='wow'>Selected</span>");
                    label.css({ position: "absolute", display: "block", background: "yellow", width: "50px", height: "20px", color: "black", "font-size": "12px", padding: "3px 3px", "font-weight": "bold" });
                    $("body").append(label);
                    label.css("top", ($(foundElement).offset().top - 5) + "px");
                    label.css("left", ($(foundElement).offset().left - 5) + "px");
                }
                if (trueEl.childNodes[indexList[i]].childNodes.length == 0) {
                    //        console.log("-----------TEXT?-------");
                    //        console.log($(trueEl.childNodes[indexList[i]]));
                    $(trueEl.childNodes[indexList[i]]).replaceWith("<hilight>" + $(realNList[i]).text() + "</hilight>");
                }
                $(realNList[i]).css("background-color", "yellow");
                trueEl.childNodes[indexList[i]].style.backgroundColor = "yellow";
                //    console.log(startIndex);
                this.addToHighLights(trueEl.childNodes[indexList[i]], indexList[i], -1);
            }
        }
    };
    MarqueeSelection.prototype.addToHighLights = function (el, txtindx, wordindx) {
        var index = $(el.tagName).index(el);
        var obj = { type: "marquee", tagName: el.tagName, index: index };
        if (el.tagName == "WORD" || el.tagName == "HILIGHT") {
            var par = el.attributes[0]["ownerElement"].parentElement;
            if (el.tagName == "WORD") {
                var startIndex = Array.prototype.indexOf.call(el.parentElement.childNodes, el);
                par = par.parentElement;
                obj["wordIndx"] = wordindx;
            }
            var parIndex = $(par.tagName).index(par);
            obj["par"] = par.tagName;
            obj["parIndex"] = parIndex;
            obj["txtnIndx"] = txtindx;
            obj["val"] = el;
        }
        this.selectedElements.push(obj);
    };
    MarqueeSelection.prototype.intersectWith = function (el) {
        //checks if element is intersecting with selection range 
        if (!el) {
            return false;
        }
        ;
        var bx1 = this._startX;
        var bx2 = this._endX;
        var by1 = this._startY;
        var by2 = this._endY;
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
    //does not need myEl
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
                if (!(ax1 >= this._startX &&
                    ax2 <= this._endX &&
                    ay1 >= this._startY &&
                    ay2 <= this._endY)) {
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
            if (rectX["left"] >= this._startX &&
                rectX["left"] + realWidth <= this._endX &&
                rectX["top"] >= this._startY &&
                rectX["top"] + realHeight <= this._endY) {
                //      this.setTextStyle(myEl, el);
                return true;
            }
            return false;
        }
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
    MarqueeSelection.prototype.findCommonParent = function () {
        if (this._parentList.length != 1) {
            for (var i = 1; i < this._parentList.length; i++) {
                var currAn = this.commonParent(this._parentList[0], this._parentList[i]);
                this._parentList[0] = currAn;
            }
        }
    };
    MarqueeSelection.prototype.commonParent = function (node1, node2) {
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
    MarqueeSelection.prototype.findParentList = function () {
        this._parentList = [];
        var el = document.elementFromPoint(this._startX, this._startY);
        this._nextX = this._endX;
        this._nextY = this._startY;
        //      this._parentList.push(el);
        if (el != null)
            this.findNextElement(el);
        console.info(this._parentList);
    };
    MarqueeSelection.prototype.findNextElement = function (el) {
        var rect = el.getBoundingClientRect();
        var nextX = this._endX - (rect.left + rect.width);
        var nextY = this._endY - (rect.top + rect.height);
        if (this._parentList.indexOf(el) > -1)
            return;
        this._parentList.push(el);
        console.info(el);
        if (el.nodeName == "HTML")
            return;
        if (nextX > 0) {
            console.log("more on the X AXIS");
            console.info(document.elementFromPoint(this._endX - nextX + 1, this._nextY));
            this.findNextElement(document.elementFromPoint(this._endX - nextX + 1, this._nextY));
        }
        if (nextY > 0) {
            console.log("more on the Y AXIS");
            console.info(document.elementFromPoint(rect.left, this._endY - nextY + 1));
            this.findNextElement(document.elementFromPoint(rect.left, this._endY - nextY + 1));
        }
    };
    return MarqueeSelection;
})(AbstractSelection);
var NullSelection = (function (_super) {
    __extends(NullSelection, _super);
    function NullSelection() {
        _super.call(this);
        this._startX = 0;
        this._startY = 0;
        this._endX = 0;
        this._endY = 0;
        this._content = "";
        console.log("Line SELECTION");
    }
    NullSelection.prototype.start = function (x, y) {
        this._startX = x;
        this._startY = y;
        console.log("line start" + x + ":" + y);
    };
    NullSelection.prototype.end = function (x, y) {
        this._endX = x;
        this._endY = y;
        this.analyzeContent();
    };
    NullSelection.prototype.getContent = function () {
        return this._content;
    };
    NullSelection.prototype.isPointAbove = function (p) {
        return false;
    };
    NullSelection.prototype.analyzeContent = function () {
        console.log("null selection... only for image");
        var img = document.elementFromPoint(this._startX, this._startY);
        console.info(img);
        console.log(img.tagName);
        if (img.tagName == "IMG") {
            console.log("IMAGE SELECTED!");
            var index = $(img.tagName).index(img);
            var obj = { type: "null", tagName: img.tagName, index: index };
            $(img).attr("src", $(img).prop('src'));
            $(img).removeAttr("srcset");
            this._content = $(img).prop('outerHTML');
            var label = $("<span class='wow'>Selected</span>");
            label.css({ position: "absolute", display: "block", background: "yellow", width: "50px", height: "20px", color: "black", "font-size": "12px", padding: "3px 3px", "font-weight": "bold" });
            $("body").append(label);
            label.css("top", ($(img).offset().top - 5) + "px");
            label.css("left", ($(img).offset().left - 5) + "px");
            this.selectedElements.push(obj);
        }
    };
    return NullSelection;
})(AbstractSelection);
//var range = window.getSelection().getRangeAt(0),
//    content = range.extractContents(),
//    span = document.createElement('SPAN');
//span.appendChild(content);
//var htmlContent = span.innerHTML;
//range.insertNode(span); 
var StrokeClassifier = (function () {
    function StrokeClassifier() {
    }
    StrokeClassifier.getStrokeType = function (stroke) {
        var p0 = stroke.points[0];
        var p1 = stroke.points[stroke.points.length - 1];
        var metrics = stroke.getStrokeMetrics();
        //      console.log("====================================="+metrics.error);
        if (Math.abs(p1.x - p0.x) < 5 && Math.abs(p1.y - p0.y) < 5) {
            return StrokeType.Null;
        }
        if (metrics.error > 50) {
            return StrokeType.Lasso;
        }
        if (Math.abs(p1.y - p0.y) < 20) {
            return StrokeType.Line;
        }
        if (Math.abs(p1.x - p0.x) < 20) {
            return StrokeType.Bracket;
        }
        if (Math.abs(p1.x - p0.x) > 50 && Math.abs(p1.y - p0.y) > 20) {
            return StrokeType.Marquee;
        }
    };
    return StrokeClassifier;
})();
var Line = (function () {
    //line in the form of  --- Ax + By = C 
    function Line(p1, p2) {
        this.p1 = p1;
        this.p2 = p2;
        this.A = p2.y - p1.y;
        this.B = p1.x - p2.x;
        this.C = p2.y * p1.x - p2.x * p1.y;
    }
    Line.prototype.hasPoint = function (p) {
        return (Math.min(this.p1.x, this.p2.x) <= p.x) && (Math.max(this.p1.x, this.p2.x) >= p.x)
            && (Math.min(this.p1.y, this.p2.y) <= p.y) && (Math.max(this.p1.y, this.p2.y) >= p.y);
    };
    return Line;
})();
var Rectangle = (function () {
    function Rectangle(x, y, w, h) {
        this.x = x;
        this.y = y;
        this.w = w;
        this.h = h;
    }
    Rectangle.prototype.getLines = function () {
        var lines = [];
        lines.push(new Line(new Point(this.x, this.y), new Point(this.x + this.w, this.y)));
        lines.push(new Line(new Point(this.x, this.y), new Point(this.x, this.y + this.h)));
        lines.push(new Line(new Point(this.x + this.w, this.y + this.h), new Point(this.x + this.w, this.y)));
        lines.push(new Line(new Point(this.x + this.w, this.y + this.h), new Point(this.x, this.y + this.h)));
        return lines;
    };
    Rectangle.prototype.hasPoint = function (p) {
        return p.x >= this.x && p.x <= this.x + this.w && p.y >= this.y && p.y <= this.y + this.h;
    };
    Rectangle.prototype.intersectsRectangle = function (r2) {
        return !(r2.x > this.x + this.w ||
            r2.x + r2.w < this.x ||
            r2.y > this.y + this.h ||
            r2.y + r2.h < this.y);
    };
    return Rectangle;
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
//# sourceMappingURL=cTed.js.map