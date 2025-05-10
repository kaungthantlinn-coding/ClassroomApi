// Function to update course theme
async function updateCourseTheme(courseId, themeColor, headerImage) {
  try {
    // Get the authentication token from localStorage
    const token = localStorage.getItem('token');
    
    if (!token) {
      throw new Error('Authentication token not found');
    }

    // Prepare the request data
    const themeData = {
      courseId: courseId,
      themeColor: themeColor,
      headerImage: headerImage
    };

    // Make the API request
    const response = await fetch('http://localhost:5203/api/courses/theme', {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify(themeData)
    });

    // Check if the request was successful
    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || 'Failed to update course theme');
    }

    // Return the success response
    return await response.json();
  } catch (error) {
    console.error('Error updating course theme:', error);
    throw error;
  }
}

// Example usage in a React component
/*
import React, { useState } from 'react';
import { updateCourseTheme } from './courseThemeUpdate';

function CourseCustomizer({ courseId }) {
  const [selectedColor, setSelectedColor] = useState('');
  const [headerImage, setHeaderImage] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const handleColorSelect = (color) => {
    setSelectedColor(color);
  };

  const handleImageUpload = (imageUrl) => {
    setHeaderImage(imageUrl);
  };

  const handleSubmit = async () => {
    setIsLoading(true);
    setError('');
    setSuccess('');

    try {
      await updateCourseTheme(courseId, selectedColor, headerImage);
      setSuccess('Course theme updated successfully!');
    } catch (error) {
      setError(error.message || 'Failed to update course theme');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="course-customizer">
      <h2>Customize Course Appearance</h2>
      
      <div className="theme-section">
        <h3>Select Theme Color</h3>
        <div className="color-options">
          {['#1976d2', '#2e7d32', '#d32f2f', '#ed6c02', '#0097a7', '#9c27b0', '#2196f3', '#757575'].map((color) => (
            <div
              key={color}
              className={`color-option ${selectedColor === color ? 'selected' : ''}`}
              style={{ backgroundColor: color }}
              onClick={() => handleColorSelect(color)}
            />
          ))}
        </div>
      </div>

      <div className="image-section">
        <h3>Select Header Image</h3>
        <div className="image-upload">
          <input type="checkbox" id="use-photo" />
          <label htmlFor="use-photo">Select photo</label>
          
          <button className="upload-button">
            Upload photo
          </button>
        </div>
      </div>

      {error && <div className="error-message">{error}</div>}
      {success && <div className="success-message">{success}</div>}

      <div className="actions">
        <button className="cancel-button">Cancel</button>
        <button 
          className="save-button" 
          onClick={handleSubmit}
          disabled={isLoading}
        >
          {isLoading ? 'Saving...' : 'Save'}
        </button>
      </div>
    </div>
  );
}

export default CourseCustomizer;
*/
