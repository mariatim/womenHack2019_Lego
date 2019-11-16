
#%%% CURL function to run in your command line

# When you CURL save all the information into: example-output-video.json
# To do that; add ">> example-output-video.json" at the end of the given CURL (as in challenge description)

# If you are using postman export the data to example-output-video.json file
#%% Download images to computer with comments
import xlsxwriter
import json
import os

if not os.path.exists('images'):
    os.makedirs('images')

with open('response2.json') as json_file:
    data = json.load(json_file)

import urllib.request
print('Beginning file download with urllib2...')

workbook = xlsxwriter.Workbook('metadata_video.xlsx')
worksheet = workbook.add_worksheet()

for i in range(0,len(data['hits']['hits'])):

    print("File number:'",i)
    try:
        url = data['hits']['hits'][i]['_source']['generatedCoverImage']
        id = data['hits']['hits'][i]['_source']['id']
        try:
            urllib.request.urlretrieve(url, filename = 'images/'+ id + '.jpg')
            worksheet.write(i, 0, id)
            worksheet.write(i, 1, url)
            worksheet.write(i, 2, data['hits']['hits'][i]['_source']['description'])
        except:
            print('Error, no url available')
    except:
        print('Error, no data')
workbook.close()
