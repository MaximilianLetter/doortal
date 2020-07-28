#include <jni.h>
#include <string>

#include <opencv2/opencv.hpp>

#define _USE_MATH_DEFINES
#include <math.h>

using namespace cv;
using namespace std;

// Resembles the Color32 format type of Unity
struct Color32
{
    uchar red;
    uchar green;
    uchar blue;
    uchar alpha;
};

// Resembles the Vector2 data type of Unity
struct Vector2
{
    float x;
    float y;
};

// Declare all used constants

// ROI constants
const float ROI_WIDTH = 0.8;
const float ROI_HEIGHT = 0.125;

// Blur constants
const Size BLUR_KERNEL = Size(3, 3);
const float BLUR_SIGMA = 2.5;

// Canny constants
const double CANNY_LOWER = 0.33; // NOTE: The lower threshold is lower than most canny auto thresholds, but necessary to catch some door edges
const double CANNY_UPPER = 1.33;

// Corner detection constants
const int CORNERS_MAX = 100;
const float CORNERS_BOT_QUALITY = 0.01;
const float CORNERS_TOP_QUALITY = 0.01;
const float CORNERS_MIN_DIST = 12.0;

// Hough line constants
const int HOUGH_LINE_WIDTH = 5;
const int HOUGH_LINE_ADDITIONAL_WIDTH = 2;
const int HOUGH_LINE_WIDTH_MAX = 20;
const float HOUGH_LINE_DIFF_THRESH_PIXEL = 15;
const float HOUGH_LINE_DIFF_THRESH_ANGLE = 0.25;
const int HOUGH_COUNT_LIMIT = 20;

// Vertical lines constants
const float LINE_MIN = 0.4;

// Rectangles constants
const float ANGLE_MAX = 0.175; // RAD
const float LENGTH_DIFF_MAX = 0.12;
const float ASPECT_RATIO_MIN = 0.3;
const float ASPECT_RATIO_MAX = 0.6; // from 0.6
const float LENGTH_HOR_DIFF_MAX = 1.2;
const float LENGTH_HOR_DIFF_MIN = 0.7;
const float RECTANGLE_THRESH = 10.0;
const float RECTANGLE_OPPOSITE_THRESH = 10.0;

// Comparison of rectangles to edges constants
const float RECT_THRESH = 0.8;
const float LINE_THRESH = 0.5;
const int LINE_WIDTH = 8;

// Selection of best candidate constants
const float GOAL_INPUT_RANGE = 0.5;
const float GOAL_RATIO = 0.45;
const float GOAL_RATIO_RANGE = 0.15;
const float GOAL_ANGLES = 90;
const float GOAL_ANGLES_DIFF_RANGE = 20;

// Declare all used functions
bool detect(Mat image, Point2f point, vector<Point2f>& result);
vector<vector<Point2f>> cornersToVertLines(vector<Point2f> cornersBot, vector<Point2f> cornersTop, vector<Vec2f> houghLines, vector<int> houghLinesWidth, Size size);
vector<vector<Point2f>> vertLinesToRectangles(vector<vector<float>>& rectInnerAngles, vector<vector<Point2f>> lines);
float compareRectangleToEdges(vector<Point2f> rect, Mat edges);
vector<Point2f> selectBestCandidate(vector<vector<Point2f>> candidates, vector<float> scores, Point2f inputPoint, vector<vector<float>> rectInnerAngles, Size size);

float getDistance(Point2f p1, Point2f p2);
float getOrientation(Point2f p1, Point2f p2);
float getCornerAngle(Point2f p1, Point2f p2, Point2f p3);
double getMedian(Mat channel);

bool detect(Mat image, Point2f inputPoint, vector<Point2f>& result)
{
    int width = image.size().width;
    int height = image.size().height;

    // Convert to grayscale
    Mat imgGray;
    cvtColor(image, imgGray, COLOR_BGR2GRAY);

    // Blur the image
    Mat blurred;
    GaussianBlur(imgGray, blurred, BLUR_KERNEL, BLUR_SIGMA);

    // Generate edges
    Mat edges;
    double median = getMedian(blurred);

    // Very dark images can go to values like 9, resulting in extremely noisy images
    median = max((double)30, median);

    double lowerThresh = max((double)0, (CANNY_LOWER * median));
    double higherThresh = min((double)255, (CANNY_UPPER * median));

    Canny(blurred, edges, lowerThresh, higherThresh);;

    // Generate hough lines
    vector<Vec2f> houghLines;
    int thresh = (int)(imgGray.size().height * 0.25);
    HoughLines(edges, houghLines, 1, CV_PI / 180, thresh, 0, 0);
    vector<Vec2f> filteredHoughLines;
    vector<int> filteredHoughLinesWidth;

    // Go through lines and merge them into bigger lines
    for (size_t h = 0; h < houghLines.size(); h++)
    {
        bool lineDone = false;

        for (int f = 0; f < filteredHoughLines.size(); f++)
        {
            Vec2f diff = houghLines[h] - filteredHoughLines[f];
            if (abs(diff[0]) < HOUGH_LINE_DIFF_THRESH_PIXEL && abs(diff[1]) < HOUGH_LINE_DIFF_THRESH_ANGLE)
            {
                filteredHoughLines[f] = (filteredHoughLines[f] + houghLines[h]) / 2;
                int width = filteredHoughLinesWidth[f] + HOUGH_LINE_ADDITIONAL_WIDTH;
                filteredHoughLinesWidth[f] = min(width, HOUGH_LINE_WIDTH_MAX);
                lineDone = true;
                break;
            }
        }

        if (lineDone) continue;

        filteredHoughLines.push_back(houghLines[h]);
        filteredHoughLinesWidth.push_back(HOUGH_LINE_WIDTH);
    }

    // Find ROI's based on user input
    Mat maskBot, maskTop;
    vector<Point2f> cornersBot, cornersTop;

    // Bottom ROI
    int roiBotWidth = width * ROI_WIDTH;
    int roiBotHeight = height * ROI_HEIGHT;
    Point2f roiPoint = Point2f(inputPoint.x - roiBotWidth / 2, inputPoint.y - roiBotHeight / 2);
    Rect roiBot = Rect(roiPoint.x, roiPoint.y, roiBotWidth, roiBotHeight);

    // Cut overlapping parts off
    roiBot = roiBot & Rect(0, 0, width, height);

    maskBot = Mat::zeros(image.size(), CV_8U);
    maskBot(roiBot) = 1;

    // Top ROI
    int lowLineBot = roiBot.y + roiBot.height;
    int roiTopHeight = lowLineBot - (LINE_MIN * height);

    Point polygonPoints[4] = {
            Point(0, 0),
            Point(width, 0),
            Point(roiBot.x + roiBot.width, roiTopHeight),
            Point(roiBot.x, roiTopHeight)
    }; // NOTE: order matters

    maskTop = Mat::zeros(image.size(), CV_8U);
    fillConvexPoly(maskTop, polygonPoints, 4, cv::Scalar(255));

    // Find corners using the given masks
    goodFeaturesToTrack(blurred, cornersBot, CORNERS_MAX, CORNERS_BOT_QUALITY, CORNERS_MIN_DIST, maskBot, 3);
    goodFeaturesToTrack(blurred, cornersTop, CORNERS_MAX, CORNERS_TOP_QUALITY, CORNERS_MIN_DIST, maskTop, 3);

    // Connect corners to vertical lines
    vector<vector<Point2f>> lines = cornersToVertLines(cornersBot, cornersTop, filteredHoughLines, filteredHoughLinesWidth, imgGray.size());

    // Group corners based on found lines to rectangles
    vector<vector<float>> rectInnerAngles;
    vector<vector<Point2f>> rectangles = vertLinesToRectangles(rectInnerAngles, lines);

    // NOTE: this could be done in vertLinesToRectangles aswell
    // Compare the found rectangles to the edge image
    vector<vector<Point2f>> candidates;
    vector<vector<float>> updInnerAngles;
    vector<float> scores;
    for (int i = 0; i < rectangles.size(); i++)
    {
        float result = compareRectangleToEdges(rectangles[i], edges);

        if (result > RECT_THRESH)
        {
            candidates.push_back(rectangles[i]);
            updInnerAngles.push_back(rectInnerAngles[i]);
            scores.push_back(result);
        }
    }
    rectInnerAngles = updInnerAngles;

    // Select the best candidate out of the given rectangles
    if (candidates.size())
    {
        vector<Point2f> door = selectBestCandidate(candidates, scores, inputPoint, rectInnerAngles, imgGray.size());

        result = door;

        return true;
    }

    return false;
}

// Group corners to vertical lines that represent the door posts
vector<vector<Point2f>> cornersToVertLines(vector<Point2f> cornersBot, vector<Point2f> cornersTop, vector<Vec2f> houghLines, vector<int> houghLinesWidth, Size size)
{
    vector<vector<Point2f>> lines;

    Mat houghMat;
    Rect fullRect = Rect(cv::Point(), size);
    /*int linesComputed = 0;*/

    for (size_t h = 0; h < houghLines.size(); h++)
    {
        houghMat = Mat::zeros(size, CV_8U);

        float rho = houghLines[h][0], theta = houghLines[h][1];

        Point pt1, pt2;
        double a = cos(theta), b = sin(theta);
        double x0 = a * rho, y0 = b * rho;
        pt1.x = cvRound(x0 + 1000 * (-b));
        pt1.y = cvRound(y0 + 1000 * (a));
        pt2.x = cvRound(x0 - 1000 * (-b));
        pt2.y = cvRound(y0 - 1000 * (a));

        float angle = abs(atan2(pt2.y - pt1.y, pt2.x - pt1.x) * 180.0 / CV_PI);
        if (angle < 80 || angle > 100)
        {
            continue;
        }
        //linesComputed++;

        //// houghLines are ordered by votes, therefor weaker lines can be omitted
        //if (linesComputed > HOUGH_COUNT_LIMIT)
        //{
        //	continue;
        //}

        line(houghMat, pt1, pt2, 1, houghLinesWidth[h], LINE_AA);

        vector<Point2f> houghPoints = {};
        for (int i = 0; i < cornersTop.size(); i++)
        {
            if (fullRect.contains(cornersTop[i]) && houghMat.at<uchar>(cornersTop[i]))
            {
                for (int j = 0; j < cornersBot.size(); j++)
                {
                    if (fullRect.contains(cornersBot[j]) && houghMat.at<uchar>(cornersBot[j]))
                    {
                        vector<Point2f> line = { cornersTop[i], cornersBot[j] };
                        lines.push_back(line);
                    }
                }
            }
        }
    }

    return lines;
}

// Group rectangles that represent door candidates out of vertical lines
vector<vector<Point2f>> vertLinesToRectangles(vector<vector<float>>& rectInnerAngles, vector<vector<Point2f>> lines)
{
    vector<vector<Point2f>> rects;

    for (int i = 0; i < lines.size(); i++)
    {
        float length1 = getDistance(lines[i][0], lines[i][1]);

        for (int j = 0; j < lines.size(); j++)
        {
            if (j <= i) continue;

            // Only build rectangle if the two lines are completely distinct
            if ((lines[i][0] == lines[j][0]) || (lines[i][0] == lines[j][1]) || (lines[i][1] == lines[j][0]) || (lines[i][1] == lines[j][1]))
            {
                continue;
            }

            // Check if length difference of lines is close
            float length2 = getDistance(lines[j][0], lines[j][1]);
            float lengthDiff = abs(length1 - length2);
            float lengthAvg = (length1 + length2) / 2;

            if (lengthDiff > (lengthAvg * LENGTH_DIFF_MAX))
            {
                continue;
            }

            // Check if top distance is in range of the given aspect ratio
            float lengthMin = lengthAvg * ASPECT_RATIO_MIN;
            float lengthMax = lengthAvg * ASPECT_RATIO_MAX;

            float distanceTop = getDistance(lines[i][0], lines[j][0]);
            if (distanceTop < lengthMin || distanceTop > lengthMax)
            {
                continue;
            }

            // Check if bottom distance is similar to top distance
            float distanceBot = getDistance(lines[i][1], lines[j][1]);
            if (distanceBot > (distanceTop * LENGTH_HOR_DIFF_MAX)
                || distanceBot < (distanceTop * LENGTH_HOR_DIFF_MIN))
            {
                continue;
            }

            // NOTE: these tests might not be necessary if corner angle test exists
            // Test orientation of top horizontal line
            float orientationTop = getOrientation(lines[i][0], lines[j][0]);
            if (orientationTop > ANGLE_MAX)
            {
                continue;
            }

            // Test orientation of bottom horizontal line
            float orientationBot = getOrientation(lines[i][0], lines[j][0]);
            if (orientationBot > ANGLE_MAX)
            {
                continue;
            }


            // These angles could be reused for voting and should be saved
            vector<float> angles;
            angles.push_back(getCornerAngle(lines[i][1], lines[i][0], lines[j][0]));
            angles.push_back(getCornerAngle(lines[i][0], lines[j][0], lines[j][1]));
            angles.push_back(getCornerAngle(lines[j][0], lines[j][1], lines[i][1]));
            angles.push_back(getCornerAngle(lines[j][1], lines[i][1], lines[i][0]));

            bool rectangular = true;

            for (int k = 0; k < 4; k++)
            {
                if (abs(90.0 - angles[k]) > RECTANGLE_THRESH)
                {
                    int kOpp = (k + 2) % 4;

                    if (abs(180.0 - (angles[k] + angles[kOpp]) > RECTANGLE_OPPOSITE_THRESH))
                    {
                        rectangular = false;
                        break;
                    }
                }
            }

            if (!rectangular) continue;

            // Sort in order: leftBot > leftTop > rightTop > rightBot
            vector<Point2f> group = { lines[i][1], lines[i][0], lines[j][0], lines[j][1] };
            rects.push_back(group);
            rectInnerAngles.push_back(angles);
        }
    }

    return rects;
}

// Compare a possible rectangle with the existing edges in the edge image
float compareRectangleToEdges(vector<Point2f> rect, Mat edges)
{
    float result = 0.0;

    for (int i = 0; i < rect.size() - 1; i++)
    {
        // Next point to connect
        int j = (i + 1) % 4;

        Mat mask = Mat::zeros(edges.size(), CV_8U);
        line(mask, rect[i], rect[j], 1, LINE_WIDTH);

        // While this works, there might be a better option without copy
        Mat roi;
        edges.copyTo(roi, mask);

        float lineLength = getDistance(rect[i], rect[j]);
        float fillRatio = min(float(1.0), countNonZero(roi) / lineLength);

        if (fillRatio < LINE_THRESH)
        {
            return 0.0;
        }

        result += fillRatio;
    }

    // Get average fillRatio for all lines but bottom line
    result = result / 3;

    return result;
}

// Select the candidate by comparing their scores, score boni if special requirements are met
vector<Point2f> selectBestCandidate(vector<vector<Point2f>> candidates, vector<float> scores, Point2f inputPoint, vector<vector<float>> rectInnerAngles, Size size)
{
    cout << candidates.size() << "size" << endl;
    float goalInputRange = size.width * GOAL_INPUT_RANGE;

    for (int i = 0; i < candidates.size(); i++)
    {
        // INPUT SCORE
        Point2f bottomCenter = (candidates[i][3] + candidates[i][0]) * 0.5;
        float inputDistance = getDistance(bottomCenter, inputPoint);
        float inputScore = (goalInputRange - inputDistance) / goalInputRange;

        // ASPECT SCORE
        float lineLeft = getDistance(candidates[i][0], candidates[i][1]);
        float lineTop = getDistance(candidates[i][1], candidates[i][2]);
        float lineRight = getDistance(candidates[i][2], candidates[i][3]);
        float lineBot = getDistance(candidates[i][3], candidates[i][0]);

        float aspectRatio = ((lineTop + lineBot) * 0.5) / ((lineLeft + lineRight) * 0.5);

        float aspectScore = (GOAL_RATIO_RANGE - abs(GOAL_RATIO - aspectRatio)) / GOAL_RATIO_RANGE;

        // ANGLE SCORE
        // NOTE: maybe punish single angle breakouts more
        float angle0 = rectInnerAngles[i][0];
        float angle1 = rectInnerAngles[i][1];
        float angle2 = rectInnerAngles[i][2];
        float angle3 = rectInnerAngles[i][3];

        float angleDiff = abs(GOAL_ANGLES - angle0) + abs(GOAL_ANGLES - angle1) + abs(GOAL_ANGLES - angle2) + abs(GOAL_ANGLES - angle3);
        float angleScore = (GOAL_ANGLES_DIFF_RANGE - angleDiff) / GOAL_ANGLES_DIFF_RANGE;


        scores[i] *= (1 + ((inputScore * 0.45 + aspectScore * 0.35 + angleScore * 0.2) * 0.5));
        /*cout << "ENDSCORE: " << scores[i] << endl;
        cout << "______________________" << endl;*/
    }

    int index = max_element(scores.begin(), scores.end()) - scores.begin();
    //cout << " winner " << index;
    vector<Point2f> door = candidates[index];

    return door;
}

// Get the distance between two points
float getDistance(Point2f p1, Point2f p2)
{
    return sqrt(pow((p1.x - p2.x), 2) + pow((p1.y - p2.y), 2));
}

// Get the orientation of the line consisting of two points
float getOrientation(Point2f p1, Point2f p2)
{
    if (p1.x != p2.x)
    {
        return abs((2 / M_PI) * atan(abs(p1.y - p2.y) / abs(p1.x - p2.x)));
    }
    else
    {
        return 180.0;
    }
}

// Get the angle between three points forming two lines
float getCornerAngle(Point2f p1, Point2f p2, Point2f p3)
{
    Point2f p12 = p1 - p2;
    Point2f p32 = p3 - p2;

    float angle = p12.dot(p32) / (norm(p12) * norm(p32));
    angle = abs(acos(angle) * 180 / M_PI);

    return angle;
}

// Calculates the median value of a single channel
// based on https://github.com/arnaudgelas/OpenCVExamples/blob/master/cvMat/Statistics/Median/Median.cpp
double getMedian(cv::Mat channel)
{
    double m = (channel.rows * channel.cols) / 2;
    int bin = 0;
    double med = -1.0;

    int histSize = 256;
    float range[] = { 0, 256 };
    const float* histRange = { range };
    bool uniform = true;
    bool accumulate = false;
    cv::Mat hist;
    cv::calcHist(&channel, 1, 0, cv::Mat(), hist, 1, &histSize, &histRange, uniform, accumulate);

    for (int i = 0; i < histSize && med < 0.0; ++i)
    {
        bin += cvRound(hist.at< float >(i));
        if (bin > m && med < 0.0)
            med = i;
    }

    return med;
}

// Below code is callable from Unity
extern "C" {
    bool ProcessImage(Vector2* result, Color32* rawImage, Vector2 userInput, int width, int height, bool rotation)
    {
        // Form input values to OpenCV types
        // NOTE: the image comes in rotated, hence height and width are flipped
        Mat image(height, width, CV_8UC4, rawImage);
        // NOTE: options to make rawImage a reference, Color32** rawImage
        //Mat inImage(height, width, CV_8UC4, *rawImage);
        //Mat image;
        //inImage.copyTo(image);

        vector<Point2f> door;
        Point2f inputPoint = Point2f(userInput.x, userInput.y);

        if (rotation) {
            // NOTE: the image is already flipped in C#
            //flip(image, image, 2);
            rotate(image, image, ROTATE_90_CLOCKWISE);
            resize(image, image, Size(image.cols, image.rows));
        }

        bool success = detect(image, inputPoint, door);

        if (success)
        {
            // Write found door corners into referenced result array
            for (int i = 0; i < 4; i++)
            {
                Vector2 &vec = result[i];
                vec.x = door[i].x;
                vec.y = door[i].y;
            }

            return true;
        }

        return false;
    }
}
