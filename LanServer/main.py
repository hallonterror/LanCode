import json
from flask import Flask, render_template, request
import requests
app = Flask(__name__)

active_users = []
active_users.append({
  'name': 'test_user',
  'games': ['game_one_dict', 'game_two_dict'],
  'email': 'test@test.com'
})
active_users.append({
  'name': 'test_user_two',
  'games': ['game_one_dict', 'game_two_dict'],
  'email': 'test@test.com'
})

@app.route('/')
def index():
  return render_template('index.html')

# @app.route('/<string:page_name>/')
# def render_static(page_name):
#     return render_template('%s.html' % page_name)

@app.route('/games/running/count')
def getNumberOfRunningGames():
  gameCount = 0
  for user in active_users:
    gameCount += len(user['games'])
  return str(gameCount)

def getGameInfo(steamid):
  addr = 'https://store.steampowered.com/api/appdetails?appids=' + str(steamid) + '&l=en'
  res = requests.get(addr)
  _, content = res.json().popitem()
  if content['success']:
    data = content['data']
    d = {
      'Title': data['name'],
      'Link': "https://store.steampowered.com/app/" + str(steamid),
      'ImgUrl': data['header_image'],
      'Description': data['short_description'],
      'IsFree': data['is_free']
    }
    return d
  else:
    d = {
      'Title': 'Uknown',
      'Link': '#',
      'ImgUrl': '#',
      'Description': 'No description',
      'IsFree': False
    }
    return d
@app.route('/games/<int:steamid>/info')
def getGameInfoJson(steamid):
  return json.dumps(getGameInfo(steamid))

@app.route('/games/<int:steamid>/image')
def getGameImage(steamid):
  addr = 'https://store.steampowered.com/api/appdetails?appids=' + str(steamid) + '&l=en'
  res = requests.get(addr)
  _, content = res.json().popitem()
  if content['success']:
    data = content['data']
    img_url = data['header_image']
    return '<img src="' + img_url + '" />'
  
@app.route('/games/all')
def getGames():
  with open('LanServer/static/games.json', 'rb') as f:
    data = json.load(f)
  return json.dumps(data)

@app.route('/games/all/update')
def updateGames():
  with open('LanServer/static/games.json', 'r') as f:
    data = json.load(f)
  
  for game in data['Games']:
    if 'SteamID' in game:
      info = getGameInfo(game['SteamID'])
      game.update(info)
      if info['IsFree'] == True:
        game['Cost'] = 'Free'
  
  with open('LanServer/static/games.json', 'w') as f:
   json.dump(data, f)

  return 'Games updated'

@app.route('/users/<string:user_id>', methods = ['GET', 'POST'])
def user(user_id):
  if request.method == 'GET':
    for user in active_users:
      if user['name'] == user_id:
        return json.dumps(user)
  elif request.method == 'POST':
    for user in active_users:
      if user_id == user['name']:
        print(request.form)
        #user['games'] = request.form
        return ''

@app.route('/users/get')
def getActiveUsers():
  return json.dumps(active_users)

@app.route('/users/names')
def getActiveUserNames():
  users = []
  for user in active_users:
    users.append(user['name'])
  return json.dumps(users)

if __name__ == '__main__':
  app.run(debug=True)