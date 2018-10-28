import json
from flask import Flask, render_template, request
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