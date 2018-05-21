# -*- coding: utf-8 -*-
"""
Created on Fri May 18 16:53:07 2018

@author: Юра
"""

import cv2
import numpy as np
import os
import time
import pickle
import math
import face_recognition

facial_features = [
        'left_eyebrow',
        'right_eyebrow',
        'nose_bridge',
        'nose_tip',
        'left_eye',
        'right_eye',
        'top_lip',
        'bottom_lip'
    ]	

def get_key_points(face_landmarks):
    res = []
    for facial_feature in facial_features:
        #print(face_landmarks[facial_feature])
        for i in range(1):
            res.append(cv2.KeyPoint(face_landmarks[facial_feature][i][0],face_landmarks[facial_feature][i][1],5))
    return res
        
        
def get_img_paths_and_responses(data_path):
    im_paths = []
    responses = []
    paths = os.listdir(data_path)
    for i in paths:
        names = os.listdir(data_path+"/"+i)
        for j in names:
            im_paths.append(data_path+'/'+i+'/'+j)
            if(i=="you_faces"):
                responses.append(1)
            if(i=="unknown_faces"):
                responses.append(2)
    return im_paths, responses

		
    
	
def train_vocabulary(img_paths,voc_size,detector):
    BOW_trainer = cv2.BOWKMeansTrainer(voc_size)
    for i in range (len(img_paths)):
        gray = cv2.imread(img_paths[i])
        face_landmarks_list = face_recognition.face_landmarks(gray)
        if(len(face_landmarks_list)==0):
            continue
        face_landmarks= face_landmarks_list[0]
        kp = get_key_points(face_landmarks)
        _,des = detector.compute(gray, kp)
        des = np.float32(des)
        BOW_trainer.add(des)  
    vocabulary = BOW_trainer.cluster()
    return vocabulary
	
	
def feature_extract(path, bow_extr,detector):
    gray = cv2.imread(path)
    face_landmarks_list = face_recognition.face_landmarks(gray) 
    face_landmarks = face_landmarks_list[0]
    kp = get_key_points(face_landmarks)
    return bow_extr.compute(gray, kp)

def extract_train_data(img_paths, responses, bow_extr,detector):
    train_data = []
    train_responses = []
    for i in range(len(img_paths)): 
        gray = cv2.imread(img_paths[i])
        face_landmarks_list = face_recognition.face_landmarks(gray)	
        if(len(face_landmarks_list)==0):
            continue
        train_data.extend(feature_extract(img_paths[i],bow_extr,detector))

        train_responses.append(responses[i])

    return train_data,train_responses
	
	
def train_classifier(tr_data,tr_responses):
    predictor = cv2.ml.SVM_create()
    predictor.setKernel(cv2.ml.SVM_RBF)
    predictor.setType(cv2.ml.SVM_NU_SVR)
    #predictor.setP(0.1)
    predictor.setNu(0.5)
    predictor.setGamma(46)
    predictor.train(np.array(tr_data), cv2.ml.ROW_SAMPLE, np.array(tr_responses))
    return predictor

	
pathtrain="data/first"
pathtest="data/first"
detector = cv2.xfeatures2d.SURF_create ()            
voc_size = 35

train_data = []
train_responses = []
start_time = time.time()

img_paths, responses = get_img_paths_and_responses(pathtrain)
print("+")
test_img_paths, test_responses = get_img_paths_and_responses(pathtest)

print("+")
vocabulary = train_vocabulary(img_paths, voc_size,detector)
print("+")
bow_extract = cv2.BOWImgDescriptorExtractor(detector,cv2.BFMatcher(cv2.NORM_L2))
print("+")
bow_extract.setVocabulary(vocabulary)
   
print("+")
train_data,train_responses,p = extract_train_data(img_paths,responses,bow_extract,detector)
print("+")
predictor = train_classifier(train_data,train_responses)
print("+")

len = len(test_img_paths)

count = 0
j = 0
you = 0
unk = 0
ayou= 0
aunk = 0                                      

for i in range(len):

    if(test_responses[i] == 1):
        ayou += 1
    if(test_responses[i] == 2):
        aunk += 1
    j = j + 1
    _,r = predictor.predict(feature_extract(test_img_paths[i],bow_extract,detector))
    if ( math.fabs( test_responses[i]-r[0] ) < 0.5):
	    count = count + 1
    else:
        if(test_responses[i] == 1):
            you += 1
        if(test_responses[i] == 2):
            unk += 1

print("Right =",count)
print ("ALL = ",j)
print("Accur = ",count/j)
print("All unknown = ",aunk, "All you = ",ayou )
print ("ER1 =" , unk,"ER2 =  ", you)
print ("ER1(%) = " , unk/aunk,"ER2(%) = ", you/ayou)