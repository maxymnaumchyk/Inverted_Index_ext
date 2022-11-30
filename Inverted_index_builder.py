import pickle
import re
import os
import time
import numpy as np
from nltk.tokenize import word_tokenize
import nltk
from nltk.corpus import stopwords
#nltk.download('stopwords')
import concurrent.futures
from multiprocessing import Manager
import threading

def words_preprocess(text):
    # split words
    words = re.split(r'[^a-zA-Z0-9_]+', text)

    # delete stopwords and convert to lowecase
    filtered_words = []
    for w in words:
        if w not in stopwords.words():
            filtered_words.append(w.lower())
    return filtered_words


def inverted_index(filtered_words):
    inverted_index = {}
    i = 0
    for w in filtered_words:
        if w not in inverted_index:
            inverted_index[w] = []
        if w in inverted_index:
            inverted_index[w].append(i)
        i+=1
    return inverted_index

def inverted_index_extend(inver, doc_id, doc_inv):
    for w, location in doc_inv.items():
        if w not in inver:
            inver[w] = {}
        if w in doc_inv:
            inver[w][doc_id] = location
    return inver

def add_doc_to_inv(inv_ext, doc_id, text):
    filtered_words = words_preprocess(text)
    inv = inverted_index(filtered_words)
    inv_ext = inverted_index_extend(inv_ext, doc_id, inv)
    return inv_ext

# iterate over files
documents = {}
i = 0
start = time.perf_counter()
for f in os.listdir('datasets/aclImdb/test/neg'):
    if 2351>i>2100:
        documents[f] = open(os.path.join('datasets/aclImdb/test/neg', f)).read()
    i += 1
print(f'Time spent on files reading: {time.perf_counter()-start} seconds')

#building an inverted_index
inv_ext = {}
with concurrent.futures.ThreadPoolExecutor() as executor:
    start = time.perf_counter()
    for doc_id, text in documents.items():
        executor.submit(add_doc_to_inv,inv_ext, doc_id, text)
print(f'Time spent on inverted_index creation: {time.perf_counter()-start} seconds')

#Straight forward approach
# start_time = time.time()
# for doc_id, text in documents.items():
#     add_doc_to_inv(inv_ext, doc_id, text)
# end_time = time.time()

#Saving an inverted index to a file
print('The inverted_index:', inv_ext)
with open('inverted_index.pkl', 'wb') as f:
    pickle.dump(inv_ext, f)
#print("--- %s seconds ---" % (time.time() - start_time))



