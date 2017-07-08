import json
import sqlite3
import sys


# Read a json file and return a list of dictionaries
def readJSON(filename):

	with open(filename, 'r') as f:
		products = json.load(f)

	return products

# Write a json file from a list of dictionaries
def writeJSON(products, outfile):

	with open(outfile, 'w') as f:
		json.dump(products, f)

# Read everything from the database's Product table and return it as a list of dictionaries
def readFromDB(filename):
	
	conn = sqlite3.connect(filename)
	c = conn.cursor()
	c.execute("SELECT * FROM Product")

	values = c.fetchall()
	dlist = []
	for val in values:
		dlist.append({"ID": val[0], "name": val[1], "width": val[2], "height": val[3], "depth": val[4], "img_path": val[5]})
	conn.close()
	return dlist

# Connect to database and insert every element in the product list into the Product table
def insertInDB(products, dbname, idFlag = False):
	
	conn = sqlite3.connect(dbname)
	c = conn.cursor()

	for elem in products:
		# ID not needed. If supplied and already existent, it will give an error
		if not idFlag:
			c.execute("INSERT INTO Product (Name, Width, Height, Depth, img_path) VALUES (?, ?, ?, ?, ?)", (elem["name"], elem["width"], elem["height"], elem["depth"], elem["img_path"]))
		else:
			c.execute("INSERT INTO Product (ID, Name, Width, Height, Depth, img_path) VALUES (?, ?, ?, ?, ?)", (elem["ID"], elem["name"], elem["width"], elem["height"], elem["depth"], elem["img_path"]))

	conn.commit()
	conn.close()


# Useful Combination of functions
def insertJSONintoDB(jsonFilename, dbFilename):

	json_in = readJSON(jsonFilename)
	insertInDB(json_in["contents"], dbFilename)


if __name__ == '__main__':

	# Tests ----------------------------------
	database = 'productDB.db'
	# jsonFile = 'example.json'

	# newProds = readJSON(jsonFile)
	# print newProds
	# insertInDB(newProds["contents"], database)
	readDB = readFromDB(database)
	print readDB
	# ----------------------------------------


	# arg[0] is json File, arg[1] is db File
	# try:
	# 	insertJSONintoDB(sys.argv[1], sys.argv[2])
	# except:
	# 	print "Not enough arguments, wrong order, or wrong json format"
